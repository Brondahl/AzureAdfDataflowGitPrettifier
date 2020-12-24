using NUnit.Framework;
using FluentAssertions;
using static AdfDataflowFilePrettifier.Tests.TestHelpers;

namespace AdfDataflowFilePrettifier.Tests
{
    public class VerificationTests
    {
        [Test,
         TestCase("SampleDataFlow1"),
         TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
         TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
        ]
        public void UglyDataflowFilesAreUgly(string fileNameSlug)
        {
            var originalUglyFileContents = ReadFileContents($"{fileNameSlug}_Ugly.json");
            var judgement = Prettifier.VerifyUgliness(originalUglyFileContents);
            judgement.Should().BeTrue();
        }

        [Test,
         TestCase("SampleDataFlow1"),
         TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
         TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
        ]
        public void PrettyDataflowFilesAreNotUgly(string fileNameSlug)
        {
            var originalUglyFileContents = ReadFileContents($"{fileNameSlug}_Pretty.txt");
            var judgement = Prettifier.VerifyUgliness(originalUglyFileContents);
            judgement.Should().BeFalse();
        }

        [Test,
         TestCase("SimpleTextFileWithoutTerminatingLine.txt"),
         TestCase("SimpleTextFileWithTerminatingCRLF.txt"),
         TestCase("SimpleTextFileWithTerminatingLF.txt"),
         TestCase("MultiLineTextFile_CRLF.txt"),
         TestCase("MultiLineTextFile_LF.txt"),
         TestCase("LargeFile.txt")]
        public void NonDataflowFilesAreUgly(string fullFileName)
        {
            var originalUglyFileContents = ReadFileContents(fullFileName);
            var judgement = Prettifier.VerifyUgliness(originalUglyFileContents);
            judgement.Should().BeTrue();
        }
    }
}