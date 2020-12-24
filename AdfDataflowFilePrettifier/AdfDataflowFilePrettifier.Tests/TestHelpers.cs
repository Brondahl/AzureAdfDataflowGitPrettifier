using System.IO;
using System.Diagnostics;
using FluentAssertions;
using static AdfDataflowFilePrettifier.Program;

namespace AdfDataflowFilePrettifier.Tests
{
    public class TestHelpers
    {
        internal const string RelativePathToPrettifierExe = "./AdfDataflowFilePrettifier.exe";
        internal const string RelativePathToTestFiles = "../../../TestFiles/";

        internal static string ReadFileContents(string fileName)
        {
            return File.ReadAllText(Path.Combine(RelativePathToTestFiles, fileName));
        }

        internal static Process StartProcess(string initialArguments)
        {
            var startInfo = new ProcessStartInfo(RelativePathToPrettifierExe);
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.Arguments = initialArguments;

            var process = new Process { StartInfo = startInfo };
            process.Start();

            return process;
        }

        internal static Process StartProcessAndWaitToCompletion(string initialArguments)
        {
            var process = StartProcess(initialArguments);
            process.WaitForExit();

            process.HasExited.Should().BeTrue();
            process.ExitCode.Should().Be(ExitCode_Success);

            return process;
        }

        internal static Process StartProcessWithInputFile(string fileName)
        {
            var args = $"{PrettifyArg} {FromFileArg} {RelativePathToTestFiles}{fileName}";
            return StartProcess(args);
        }

        internal static string StartProcessWithInputFile_ReadResultingStdOutToCompletion(string fileName)
        {
            var process = StartProcessWithInputFile(fileName);
            return ReadResultingStdOutToCompletion(process);
        }

        internal static Process StartProcessAndWriteToStdIn(string stdIn)
        {
            var process = StartProcess($"{PrettifyArg} {FromStandardInputStreamArg}");

            process.HasExited.Should().BeFalse();

            process.StandardInput.Write(stdIn);
            process.StandardInput.Flush();
            process.StandardInput.Close();

            return process;
        }

        internal static string StartProcess_WriteToStdIn_ReadResultingStdOutToCompletion(string stdIn)
        {
            var process = StartProcessAndWriteToStdIn(stdIn);
            return ReadResultingStdOutToCompletion(process);
        }

        internal static string ReadResultingStdOutToCompletion(Process process)
        {
            string secondOutText = null;
            if (process.StandardOutput.Peek() > -1)
            {
                secondOutText = process.StandardOutput.ReadToEnd();
            }

            // Must happen AFTER ReadToEnd().
            // It's still not *entirely* bullet-proof, but in practice, it's fine.
            // See here: https://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why
            process.WaitForExit();

            process.HasExited.Should().BeTrue();
            process.ExitCode.Should().Be(ExitCode_Success);

            secondOutText.Should().NotBeNullOrWhiteSpace();
            return secondOutText;
        }
    }
};