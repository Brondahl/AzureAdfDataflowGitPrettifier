<!-- markdownlint-disable MD025 -->
# AzureAdfDataflowGitPrettifier

A tool and config file collection for allowing neat git interactions with ADF Dataflow definition files

## TL;DR

  1. Install kdiff3 as your git mergetool: <https://sourceforge.net/projects/kdiff3/files/>
  2. Copy-Paste this code-base into `<repoRoot>/dataflowGitDiffTool`
     * You'll want to delete the `.git` folder in here, and delete or move the `.gitignore` file.
  3. Open a bash shell in the repo root, and run:
        `./dataflowGitDiffTool/SetupGitPrettification.sh`
  4. Use `gitk`, `git gui`, and `kdiff3` as your git interfaces.
  5. Whenever you run `git mergetool` select 'Use The Options During Merge' on the kdiff3 prompt.

That's about it.

It's really pretty important to understand what's going on, though, and what this tool is doing behind the scenes ....

## Context/Cause

If you're looking at this tool, then you presumably already know what ADF is, and further that ADF stores all of its pipeline definition is .json 'code-behind' files.
ADF is doing a good job of most of the files - pipelines, datasets, etc. But the files produced to define Dataflows are very problematic.
Dataflows are defined in a "script" language of sorts, which is a long multi-line string. In order to store that string in a JSON format, ADF escapes all the whitespace in that
string before storing it in the code-behind file. That's fine for the UI, since it can un-escape it before rendering or using it.

But for git that means that the entirety of the interesting definition is on a single huge line.
This makes it not meaningfully readable. Even worse, it is *entirely* un-mergable.
However the unreadable/unmergable nature of these files isn't inherent - changes to the script could *conceptually* be merged it's only the formatting applied to the script
that causes a problem.
It is (somewhat) easy to loss-lessly convert to a readable format (albeit not a valid json file), and then convert back to the expected ADF-compatible, valid-JSON format.

I've written a tool to do exactly that conversion :D and worked out the git configuration necessary to perform the conversion and anti-conversions at the right time.

Specifically, the tool locates the critical line of the json file, which holds a string with all the whitespace escaped to "\r", "\n", "\t", etc. And "unescapes" those whitespace markers.
The resulting prettified file has short lines that can be easily read, for which diffs can be easily interpretted, and for which merges are a viable possibility.

Whilst doing the prettification, the tool includes ASCII control characters to "mark" these bits of replaced whitespace, so that at the end, it can reverse the process.
This 'uglification' process locates the marked whitespace, and turns them back into escaped "non-whitespace" strings, returning the file to its original state.

The command-line tool supports a variety of invocation methods, but fortunately you shouldn't need to care about those, as git can handle it all for you, if you follow the steps below.
There is a comprehensive suite of tests, in the solution, if you do want to see how it can be called.

## Main Files

The exe used by git is beside this README:
    `<repoRoot>/dataflowGitDiffTool/AdfDataflowFilePrettifier.exe`
The sln defining the exe is here too:
    `<repoRoot>/dataflowGitDiffTool/AdfDataflowFilePrettifier/AdfDataflowFilePrettifier.sln`
    That solution copies its latest exe output up to the used location above.
    The sln consists of a 2 file Console App, and a Test suite.

    The Program.cs handles arguments and IO.
    The Prettifier.cs handles the actual string manipulation, treating the entirety of the files in question as single strings.

## Using the Prettifier with Git

The are 4 aspects to fully using this tool:

* A. Configure `textconv` so that diffs are readable.
* B. Configure mergetool to perform merge-conflict-resolution on a prettified file.
* C. Configure mergetool to re-uglify the resolved files, post merge-conflict-resolution.
* D. Configure git hooks, to ensure that no prettified files ever make their way onto remote.

All 4 steps can be achieved by simply running `<repoRoot>/dataflowGitDiffTool/SetupGitDataflowPrettification.sh`, from the git repo root.
But if you want more details ....

### A) `textconv`

To configure `textconv` (a git setting), you will need to run the following command to modify your git config file and project .gitattributes file

    git config diff.dataflowPrettifier.textconv './dataflowGitDiffTool/AdfDataflowFilePrettifier.exe -prettify -fromFile'
    echo '*.json diff=dataflowPrettifier' >> .gitattributes

If you want to set the config value globally, feel free, but local-only is probably sufficient.
These settings are part of the git standard, so any suitably well-implemented git interface should honour it.
But e.g. VS probably doesn't. Use a properly implemented git interface ¯\_(ツ)_/¯

### B) & C) Configuring the kdiff mergetool to use this.

*Prerequisite*: Install KDiff3, and have it configured as the default merge handler.
I've been able to combine the config for these 2 steps into a single process, by providing a custom mergetool.
Run the following commands to configure the tool:

    git config merge.tool kdiff3_with_uglification
    git config mergetool.keepBackup false
    git config mergetool.kdiff3_with_uglification.cmd './dataflowGitDiffTool/kdiff3_with_uglification.sh "$BASE" "$LOCAL" "$REMOTE" -o "$MERGED"'

Note that you will have already had the following in your config file.

    [merge]
        tool = kdiff3
    [mergetool "kdiff3"]
        path = C:/Program Files/KDiff3/kdiff3.exe

    The former will have been overriden, but the latter must be kept, as it's used to locate the kdiff3.exe

The custom mergetool `kdiff3_with_uglification` wraps kdiff3.
When you run `git mergetool` the custom script passes all the args on to kdiff, along with a parameter telling kdiff to pre-process the files with the prettifier (existing kdiff feature).
You then get the standard kdiff interface, chose your merge resolution and save the merged file (which is now prettified.)
When kdiff closes, before control is returned to git, the custom script uglifies the merged file, returning it back to the desited ugly state.
Other than a few extra log lines, the only observable difference is a UI pop-up in kdiff challenging whether you want to run the pre-processing for these files. Obviously you do want to :)

### D) git hooks

Run the following commands (in the git repo Root), to copy the hooks to be used as active git hooks.
Note that these are extensions-less files.

    cp -f ./dataflowGitDiffTool/pre-commit.template ./.git/hooks/pre-commit
    cp -f ./dataflowGitDiffTool/pre-push.template ./.git/hooks/pre-push

These are simple bash scripts that will execute before you commit or push, ensuring that you never commit a prettified file.
This is necessary because the output of the prettification process is not a valid JSON file, and can't be handled by ADF. So if anyone ever pushed one back to remote, it would break everything.
If you've correctly configured step C), then it shouldn't be possible for these things to happen, but this is a belt-and-braces approach.
