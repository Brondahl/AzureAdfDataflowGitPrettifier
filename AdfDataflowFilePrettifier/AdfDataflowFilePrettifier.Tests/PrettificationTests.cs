using NUnit.Framework;
using FluentAssertions;
using static AdfDataflowFilePrettifier.Tests.TestHelpers;

namespace AdfDataflowFilePrettifier.Tests
{
    public class PrettificationTests
    {
        [Test,
         TestCase("SampleDataFlow1"),
         TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
         TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
        ]
        public void PrettifyUglyFileContents(string fileNameSlug)
        {
            var originalUglyFileContents = ReadFileContents($"{fileNameSlug}_Ugly.json");
            var expectedPrettyFileContents = ReadFileContents($"{fileNameSlug}_Pretty.txt");
            var prettyOutput = Prettifier.PrettifyFileTextString(originalUglyFileContents);
            prettyOutput.Should().Be(expectedPrettyFileContents);
        }

        [Test,
         TestCase("SampleDataFlow1"),
         TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
         TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
        ]
        public void UglifyPrettyFileContents(string fileNameSlug)
        {
            var originalPrettyFileContents = ReadFileContents($"{fileNameSlug}_Pretty.txt");
            var expectedUglyFileContents = ReadFileContents($"{fileNameSlug}_Ugly.json");
            var uglyOutput = Prettifier.UglifyFileTextString(originalPrettyFileContents);
            uglyOutput.Should().Be(expectedUglyFileContents);
        }

        [Test,
         TestCase("SampleDataFlow1"),
         TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
         TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
        ]
        public void UglifyThenPrettifyIsNoOp(string fileNameSlug)
        {
            var originalPrettyFileContents = ReadFileContents($"{fileNameSlug}_Pretty.txt");
            var uglyOutput = Prettifier.UglifyFileTextString(originalPrettyFileContents);
            var prettyOutput = Prettifier.PrettifyFileTextString(uglyOutput);
            prettyOutput.Should().Be(originalPrettyFileContents);
        }

        [Test,
         TestCase("SampleDataFlow1"),
         TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
         TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
        ]
        public void PrettifyThenUglifyIsNoOp(string fileNameSlug)
        {
            var originalUglyFileContents = ReadFileContents($"{fileNameSlug}_Ugly.json");
            var prettyOutput = Prettifier.PrettifyFileTextString(originalUglyFileContents);
            var uglyOutput = Prettifier.UglifyFileTextString(prettyOutput);
            uglyOutput.Should().Be(originalUglyFileContents);
        }

        /**
         * See https://github.com/Brondahl/AzureAdfDataflowGitPrettifier/pull/1/files#r554426640
         * and https://github.com/Brondahl/AzureAdfDataflowGitPrettifier/pull/1/files#r554534909
         * Unfortunately, when kdiff resolves merges, it doesn't respect existing line endings
         * So we set it to always use \n. Most (but not all) line endings are \n, but occasionally
         * \r\n is used (see eg SampleDataFlow3), which ideally need handling without converting to \r\n
         */
        [Test,
         TestCase("SampleDataFlow1"),
         TestCase("SampleDataFlow2_WithEscapedTextNotInDataflow"),
         TestCase("SampleDataFlow3_WithEscapedCRInDefinition"),
        ]
        public void PrettifyContainsNoWindowsLineEndings(string fileNameSlug)
        {
            var originalUglyFileContents = ReadFileContents($"{fileNameSlug}_Ugly.json");
            var prettyOutput = Prettifier.PrettifyFileTextString(originalUglyFileContents);

            prettyOutput.Should().NotContain("\r\n");
        }
    }
}