using System.Linq;
using System.Text.RegularExpressions;

namespace AdfDataflowFilePrettifier
{
    public class Prettifier
    {
        /*==========================================================================
         == Note that these public methods MUST read the files as single strings. ==
         == Otherwise we run into problems with the IO methods having to try to   ==
         == handle the different possible line endings themselves, and the whole  ==
         == thing turns into a MASSIVE mess.                                      ==
         ===========================================================================*/
        public static string PrettifyFileTextString(string existingFileContents)
        {
            if (FileContainsDataFlow(existingFileContents) && !FileIsCurrentlyPretty(existingFileContents))
            {
                return PrettifyDataflowFile(existingFileContents);
            }
            else
            {
                return existingFileContents;
            }
        }

        //See above.
        public static string UglifyFileTextString(string existingFileContents)
        {
            if (FileContainsDataFlow(existingFileContents) && FileIsCurrentlyPretty(existingFileContents))
            {
                return UglifyTextRepresentingPrettifiedDataFlowFile(existingFileContents);
            }
            else
            {
                return existingFileContents;
            }
        }

        //See above.
        public static bool VerifyUgliness(string existingFileContents)
        {
            return !FileContainsDataFlow(existingFileContents) || !FileIsCurrentlyPretty(existingFileContents);
        }





        private static bool FileContainsDataFlow(string fileContents)
        {
            return fileContents.Contains("\"type\": \"MappingDataFlow\",");
        }

        private static bool FileIsCurrentlyPretty(string existingFileContents)
        {
            return existingFileContents.Contains(InnerMagicIndicatorCharacter) || existingFileContents.Contains(OuterMagicIndicatorCharacter) || existingFileContents.Contains(CRMagicIndicatorCharacter);
        }

        private static string PrettifyDataflowFile(string fileContentsToPrettify)
        {
            var (preamble, dataflowScriptDefinition, postamble) = SplitFileIntoInterestingSections(fileContentsToPrettify);

            var prettifiedDataflowScriptDefinition = PrettifyDataFlowScriptDefinition(dataflowScriptDefinition);

            return preamble + prettifiedDataflowScriptDefinition + postamble;
        }

        private static (string, string, string) SplitFileIntoInterestingSections(string fileContentsToPrettify)
        {
            var regex = new Regex($@"^(.*""script"": "")(.*)(""(?:\r|\n).*)$", RegexOptions.Singleline | RegexOptions.Compiled);
            var matches = regex.Match(fileContentsToPrettify);

            return (
                //Groups[0] is the match of the whole string.
                matches.Groups[1].Value, // preamble
                matches.Groups[2].Value, // script Definition (NOT including surrounding quote marks)
                matches.Groups[3].Value  // postamble
                );
        }


        private const char InnerMagicIndicatorCharacter = '\x05'; // We want to be able to distinguish our 'created' \n characters, etc., from the 'real' ones, so that we can reverse the process later.
        private const char OuterMagicIndicatorCharacter = '\x06'; // And we also want to do this slightly different at the edges of the script.
        private const char CRMagicIndicatorCharacter = '\x07';    // Due to limitations of kDiff we want to flag CR (\r) characters separately too.

        private static string PrettifyDataFlowScriptDefinition(string scriptLineToPrettify)
        {
            var prettierLine = scriptLineToPrettify;

            var markedNewLine = OuterMagicIndicatorCharacter + "\n";
            prettierLine = prettierLine.Replace("\\r\\n", CRMagicIndicatorCharacter + "\n");
            prettierLine = prettierLine.Replace("\\r", CRMagicIndicatorCharacter + "\r");
            prettierLine = prettierLine.Replace("\\n", InnerMagicIndicatorCharacter + "\n");
            prettierLine = prettierLine.Replace("\\t", InnerMagicIndicatorCharacter + "\t");
                
            return markedNewLine + prettierLine + markedNewLine;
        }

        internal static string UglifyTextRepresentingPrettifiedDataFlowFile(string fileContents)
        {
            var uglierFile = fileContents;

            uglierFile = uglierFile.Replace(OuterMagicIndicatorCharacter + "\n", "");
            uglierFile = uglierFile.Replace(CRMagicIndicatorCharacter + "\n", "\\r\\n");
            uglierFile = uglierFile.Replace(CRMagicIndicatorCharacter + "\r", "\\r");
            uglierFile = uglierFile.Replace(InnerMagicIndicatorCharacter + "\n", "\\n");
            uglierFile = uglierFile.Replace(InnerMagicIndicatorCharacter + "\t", "\\t");

            return uglierFile;
        }

    }
}
