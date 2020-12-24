using NUnit.Framework;
using FluentAssertions;
using static AdfDataflowFilePrettifier.Tests.TestHelpers;

namespace AdfDataflowFilePrettifier.Tests
{
    public class IoHandlingTests
    {
        [Test,
         TestCase("SimpleTextFileWithoutTerminatingLine.txt"),
         TestCase("SimpleTextFileWithTerminatingCRLF.txt"),
         TestCase("SimpleTextFileWithTerminatingLF.txt"),
         TestCase("MultiLineTextFile_CRLF.txt"),
         TestCase("MultiLineTextFile_LF.txt"),
         TestCase("LargeFile.txt")]
        public void ReproduceSimpleFilesFromFile(string fileName)
        {
            var originalFile = ReadFileContents(fileName);
            var output = StartProcessWithInputFile_ReadResultingStdOutToCompletion(fileName);
            output.Should().Be(originalFile);
        }

        [Test,
         TestCase("SimpleTextFileWithoutTerminatingLine.txt"),
         TestCase("SimpleTextFileWithTerminatingCRLF.txt"),
         TestCase("SimpleTextFileWithTerminatingLF.txt"),
         TestCase("MultiLineTextFile_CRLF.txt"),
         TestCase("MultiLineTextFile_LF.txt"),
         TestCase("LargeFile.txt")]
        public void ReproduceSimpleFilesFromStdIn(string fileName)
        {
            var originalFile = ReadFileContents(fileName);
            var output = StartProcess_WriteToStdIn_ReadResultingStdOutToCompletion(originalFile);
            output.Should().Be(originalFile);
        }
    }
}