# Largely sourced from the following:
# * https://blog.beardhatcode.be/2018/03/your-own-git-mergetool.html
# * https://git-scm.com/docs/git-mergetool
# * output of kdiff3.exe --help.
# * https://askubuntu.com/questions/763332/execute-a-command-stored-into-a-variable (used this one most directly)
# * https://stackoverflow.com/questions/4824590/propagate-all-arguments-in-a-bash-shell-script
# * https://unix.stackexchange.com/questions/444946/how-can-we-run-a-command-stored-in-a-variable/444949#444949
# * http://kdiff3.sourceforge.net/doc/preprocessors.html
set -eu

echo Using custom mergetool: kdiff3 plus custom uglification.
mergeOutput=$5
echo Target of merge: $mergeOutput

kdiffPath=$(git config --get mergetool.kdiff3.path)
if [ -z "$kdiffPath" ]; then
    echo kdiff3 is not configured in git
    echo \'git config --get mergetool.kdiff3.path\' returned: \'$kdiffPath\'
    exit 1
fi

# This will cause kdiff3 to write the files to the StdIn of the process, and use the resulting stdOut rather than the original files.
applyPreProcessorConfig="PreProcessorCmd=.\\dataflowGitDiffTool\\AdfDataflowFilePrettifier.exe -prettify -fromStdIn"

# See here: https://askubuntu.com/questions/763332/execute-a-command-stored-into-a-variable
invokeKDiff=("$kdiffPath" "$@" "--cs" "$applyPreProcessorConfig")
"${invokeKDiff[@]}"

# Capture exitCode to report it back to git.
exitCode=$?
echo kdiff3 exited with code: $exitCode

if [ "$exitCode" -eq "0" ]; then
    echo re-uglifying \'$mergeOutput\'
    # Not just write back to the same file immediate; that would cause deadlocks.
    ./dataflowGitDiffTool/AdfDataflowFilePrettifier.exe -uglify -fromFile "$mergeOutput" > "$mergeOutput.ugly"
    mv -f -u "$mergeOutput.ugly" "$mergeOutput"
    # Not 'git add -A'. That will stage the temp merge files!
    git add "$mergeOutput"
fi

exit $exitCode