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
        public void UglyDataflowFilesAreSafeForCommitting(string fileNameSlug)
        {
            var originalUglyFileContents = ReadFileContents($"{fileNameSlug}_Ugly.json");
            var judgement = Prettifier.VerifyFileSafeForCommitting(originalUglyFileContents);
            judgement.Should().BeTrue();
        }

        [Test,
         TestCase("SampleDataFlow1"),
         TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
         TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
        ]
        public void PrettyDataflowFilesAreNotSafeForCommitting(string fileNameSlug)
        {
            var originalUglyFileContents = ReadFileContents($"{fileNameSlug}_Pretty.txt");
            var judgement = Prettifier.VerifyFileSafeForCommitting(originalUglyFileContents);
            judgement.Should().BeFalse();
        }

        [Test,
         TestCase("SampleDataFlow3_WithEscapedCRInDefinition_InvalidJson.txt")
        ]
        public void DataflowFileWhichIsInvalidJsonIsNotSafeForCommitting(string fullFileName)
        {
            var fileContents = ReadFileContents(fullFileName);
            var judgement = Prettifier.VerifyFileSafeForCommitting(fileContents);
            judgement.Should().BeFalse();
        }

        [Test,
         TestCase("SimpleTextFileWithoutTerminatingLine.txt"),
         TestCase("SimpleTextFileWithTerminatingCRLF.txt"),
         TestCase("SimpleTextFileWithTerminatingLF.txt"),
         TestCase("MultiLineTextFile_CRLF.txt"),
         TestCase("MultiLineTextFile_LF.txt"),
         TestCase("LargeFile.txt")]
        public void NonDataflowFilesAreSafeForCommitting(string fullFileName)
        {
            var originalUglyFileContents = ReadFileContents(fullFileName);
            var judgement = Prettifier.VerifyFileSafeForCommitting(originalUglyFileContents);
            judgement.Should().BeTrue();
        }
    }
}