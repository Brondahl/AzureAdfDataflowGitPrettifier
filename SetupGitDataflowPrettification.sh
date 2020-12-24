#See README.md for details of what this is doing, any why.

set -eu

existingKDiffPath=$(git config --get mergetool.kdiff3.path)
if [ -z "$existingKDiffPath" ]; then
    echo kdiff3 is not configured in git
    echo kdiff3 is a pre-requisite for using this tool.
    echo \'git config --get mergetool.kdiff3.path\' returned: \'$existingKDiffPath\'
    exit 1
fi

echo Configuring 'textconv' in local git config and .gitattributes file
git config diff.dataflowPrettifier.textconv './dataflowGitDiffTool/AdfDataflowFilePrettifier.exe -prettify -fromFile'
echo '*.json diff=dataflowPrettifier' >> .gitattributes

echo Configuring 'kdiff3_with_uglification' as the mergetool
git config merge.tool kdiff3_with_uglification
git config mergetool.keepBackup false
git config mergetool.kdiff3_with_uglification.cmd './dataflowGitDiffTool/kdiff3_with_uglification.sh "$BASE" "$LOCAL" "$REMOTE" -o "$MERGED"'

echo Configuring pre-commit/push git hooks
cp -f ./dataflowGitDiffTool/pre-commit.template ./.git/hooks/pre-commit
cp -f ./dataflowGitDiffTool/pre-push.template ./.git/hooks/pre-push
