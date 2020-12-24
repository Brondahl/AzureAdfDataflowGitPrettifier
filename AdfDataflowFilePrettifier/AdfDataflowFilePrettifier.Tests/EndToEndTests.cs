using NUnit.Framework;
using FluentAssertions;
using static AdfDataflowFilePrettifier.Tests.TestHelpers;

namespace AdfDataflowFilePrettifier.Tests
{
    public class EndToEndTests
    {
        [Test,
 TestCase("SampleDataFlow1"),
 TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
 TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
]
        public void PrettifyAnUglyFileFromDisk(string fileNameSlug)
        {
            var originalUglyFileName = $"{fileNameSlug}_Ugly.json";
            var expectedPrettyFileContents = ReadFileContents($"{fileNameSlug}_Pretty.txt");
            var prettyStdOut = StartProcessWithInputFile_ReadResultingStdOutToCompletion(originalUglyFileName);
            prettyStdOut.Should().Be(expectedPrettyFileContents);
        }

        [Test,
 TestCase("SampleDataFlow1"),
 TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
 TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
]
        public void PrettifyAnUglyFileFromStdIn(string fileNameSlug)
        {
            var originalUglyFileContents = ReadFileContents($"{fileNameSlug}_Ugly.json");
            var expectedPrettyFileContents = ReadFileContents($"{fileNameSlug}_Pretty.txt");
            var prettyStdOut = StartProcess_WriteToStdIn_ReadResultingStdOutToCompletion(originalUglyFileContents);
            prettyStdOut.Should().Be(expectedPrettyFileContents);
        }
    }
}