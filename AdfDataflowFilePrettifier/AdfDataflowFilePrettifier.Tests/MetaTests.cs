using System.IO;
using NUnit.Framework;
using FluentAssertions;
using static AdfDataflowFilePrettifier.Tests.TestHelpers;
using static AdfDataflowFilePrettifier.Program;

namespace AdfDataflowFilePrettifier.Tests
{
    /// <summary>
    /// These tests aren't really testing the code of the program. They're most testing whether or not we can successfully run tests that interact with that program properly.
    /// * Can we find the exe.
    /// * Can we run that exe and run its output.
    /// * Can we simulate data being piped into that exe.
    /// * Can we BOTH pipe data in, AND read data out! (i.e. avoid various Stream deadlocking gotchas)
    /// </summary>
    public class MetaTests
    {
        private const int StabilityTest = 1;

        [Test, Repeat(StabilityTest)]
        public void Meta_CanFindBuiltExe()
        {
            File.Exists(RelativePathToPrettifierExe).Should().BeTrue();
        }

        [Test, Repeat(StabilityTest)]
        public void Meta_CanInitiateExeAndHaveItEnd()
        {
            StartProcessAndWaitToCompletion("-badInput");
        }

        [Test, Repeat(StabilityTest)]
        public void Meta_CanInitiateExeAndDetectOutput()
        {
            var process = StartProcessAndWaitToCompletion("-badInput");

            var outText = process.StandardOutput.ReadToEnd();
            var errText = process.StandardError.ReadToEnd();

            outText.Should().NotBeNullOrEmpty();
            errText.Should().BeNullOrEmpty();
        }

        [Test, Repeat(StabilityTest)]
        public void Meta_CanInitiateExeAndSendInput_Manual()
        {
            var process = StartProcess($"{PrettifyArg} {FromStandardInputStreamArg}");

            process.HasExited.Should().BeFalse();

            process.StandardInput.Write("Some Input Here");
            process.StandardInput.Flush();
            process.StandardInput.Close();

            string outputText = null;
            if (process.StandardOutput.Peek() > -1)
            {
                outputText = process.StandardOutput.ReadToEnd();
            }
            outputText.Should().NotBeNullOrEmpty();
        }

        [Test, Repeat(StabilityTest)]
        public void Meta_CanInitiateExeAndSendInput_SemiAuto()
        {
            var process = StartProcessAndWriteToStdIn("Some Input Here");

            string outputText = null;
            if (process.StandardOutput.Peek() > -1)
            {
                outputText = process.StandardOutput.ReadToEnd();
            }
            outputText.Should().NotBeNullOrEmpty();
        }

        [Test, Repeat(StabilityTest)]
        public void Meta_CanInitiateExeAndSendInput_FullAuto()
        {
            var output = StartProcess_WriteToStdIn_ReadResultingStdOutToCompletion("Some Input Here");
            output.Should().NotBeNullOrEmpty();
        }

        [Test, Repeat(StabilityTest)]
        public void Meta_CanInitiateExePointingAtFile()
        {
            var output = StartProcessWithInputFile_ReadResultingStdOutToCompletion("SimpleTextFile.txt");
            output.Should().NotBeNullOrEmpty();
        }
    }
}