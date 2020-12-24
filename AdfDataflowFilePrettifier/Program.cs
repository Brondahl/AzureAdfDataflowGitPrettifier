using System;
using System.IO;

namespace AdfDataflowFilePrettifier
{
    public class Program
    {
        public const string PrettifyArg = "-prettify";
        public const string UglifyArg = "-uglify";
        public const string VerifyArg = "-verifyIsUgly";
        public const string FromFileArg = "-fromFile";
        public const string FromStandardInputStreamArg = "-fromStdIn";

        public const int ExitCode_Success = 0;
        public const int ExitCode_Failure = 1;

        static void Main(string[] args)
        {
            if (args[0] == VerifyArg)
            {
                PerformVerification(args);
            }
            else
            {
                try
                {
                    PerformConfiguredTextConversion(args);
                    Environment.ExitCode = ExitCode_Success;
                }
                catch (Exception e)
                {
                    //Given how this will get invoked, it's really critical that it never *actually* throws, and that in the event of an error we write the details to StdOut, in a way that will get shown to the users.
                    var rand = new Random();
                    Console.WriteLine($"{rand.Next()} Random value to ensure that error diffs are different, so that it shows up in diff tools! See error below.");
                    Console.WriteLine(e.ToString());
                    Environment.ExitCode = ExitCode_Success; //See above. This is NOT a mistake. We ALWAYS assert that we were successful!
                }
            }
        }

        private static void PerformVerification(string[] args)
        {
            Func<string> getFileContentsFunc = ParseSourceArg(args);
            var fileContents = getFileContentsFunc();

            var isUgly = Prettifier.VerifyUgliness(fileContents);

            Environment.ExitCode = isUgly ? ExitCode_Success : ExitCode_Failure;
        }


        static void PerformConfiguredTextConversion(string[] args)
        {
            Func<string, string> operationToPerform = ParseConversionOperationArg(args);
            Func<string> getFileContents = ParseSourceArg(args);

            PerformOperation(getFileContents, operationToPerform);
        }

        private static Func<string, string> ParseConversionOperationArg(string[] args)
        {
            var operationArg = args[0];
            switch (operationArg)
            {
                case PrettifyArg:
                    return Prettifier.PrettifyFileTextString;
                case UglifyArg:
                    return Prettifier.UglifyFileTextString;
                default:
                    throw InvalidArgs(args);
            }
        }

        private static Func<string> ParseSourceArg(string[] args)
        {
            var sourceArg = args[1];
            Func<string> getFileContents = null;
            switch (sourceArg)
            {
                case FromStandardInputStreamArg:
                    if (args.Length != 2)
                    {
                        throw InvalidArgs(args);
                    }
                    getFileContents = ReadAllOfStdIn;
                    break;

                case FromFileArg:
                    if (args.Length != 3)
                    {
                        throw InvalidArgs(args);
                    }
                    var filePathArg = args[2];
                    getFileContents = (() => ReadFromFile(filePathArg));
                    break;

                default:
                    throw InvalidArgs(args);
            }

            return getFileContents;
        }

        private static Exception InvalidArgs(string[] args)
        {
            return new NotSupportedException("Unrecognised inputs: " + string.Join(" ", args));
        }

        static void PerformOperation(Func<string> inputSource, Func<string, string> stringOperation)
        {
            var existingFileContents = inputSource();
            var updatedFileContents = stringOperation(existingFileContents);
            PrintTextBackToStdOut(updatedFileContents);
        }

        static string ReadFromFile(string pathToFileToPrettify)
        {
            if (File.Exists(pathToFileToPrettify))
            {
                // We *don't* read this line-by-line as that would lose information about the original line-endings, which we have to preserve.
                return File.ReadAllText(pathToFileToPrettify);
            }
            else
            {
                throw new FileNotFoundException("Could not found File to Prettify.", pathToFileToPrettify);
            }
        }

        static string ReadAllOfStdIn()
        {
            // As above, we *don't* read this line-by-line to ensure that we preserve the line-ending data.
            var inputStream = new StreamReader(Console.OpenStandardInput());
            return inputStream.ReadToEnd();
        }

        private static void PrintTextBackToStdOut(string fileContentsToWrite)
        {
            // For the same reasons as above, we don't write this data line-by-line so that we preserve the line-ending data.
            Console.Write(fileContentsToWrite);
        }
    }
}
