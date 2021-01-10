using System;
using System.Collections.Generic;
using System.Text;
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
            return existingFileContents.Contains(BlockStartString) || existingFileContents.Contains(BlockEndString);
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

        private const string BlockStartString = "\n/// PRETTIFIED SCRIPT START MARKER /// DO NOT ALTER THIS LINE ///\n";
        private const string BlockEndString = "\n/// PRETTIFIED SCRIPT END MARKER /// DO NOT ALTER THIS LINE ///\n";

        private static string PrettifyDataFlowScriptDefinition(string scriptLineToPrettify)
        {
            return BlockStartString + PrettifyString(scriptLineToPrettify) + BlockEndString;
        }

        internal static string UglifyTextRepresentingPrettifiedDataFlowFile(string fileContents)
        {
            var endOfLastBlock = 0;
            var uglyFileBuilder = new StringBuilder();

            foreach(var block in GetAllBlockIndices(fileContents))
            {
                var newContentUpToBlockStart = ReadStringBetween(fileContents, endOfLastBlock, block.StartOfBlockStartMarkerIndex);
                var blockContent = ReadStringBetween(fileContents, block.BlockContentStartIndex, block.BlockContentEndIndex);

                uglyFileBuilder.Append(newContentUpToBlockStart);
                uglyFileBuilder.Append(UglifyString(blockContent));

                endOfLastBlock = block.EndOfBlockEndMarkerIndex;
            }

            // No more blocks in the file, read rest of file into string
            uglyFileBuilder.Append(fileContents.Substring(endOfLastBlock));

            return uglyFileBuilder.ToString();
        }

        private static IEnumerable<BlockIndices> GetAllBlockIndices(string str)
        {
            var nextBlockIndicies = GetNextBlockIndices(str, 0);
            while (nextBlockIndicies != null)
            {
                yield return nextBlockIndicies;
                nextBlockIndicies = GetNextBlockIndices(str, nextBlockIndicies.EndOfBlockEndMarkerIndex);
            }
        }

        private static BlockIndices GetNextBlockIndices(string str, int previousBlockEndIndex)
        {
            var startOfBlockStartMarkerIndex = str.IndexOf(BlockStartString, previousBlockEndIndex);

            if (startOfBlockStartMarkerIndex == -1)
            {
                return null;
            }

            var blockContentStartIndex = startOfBlockStartMarkerIndex + BlockStartString.Length;
            var blockContentEndIndex = str.IndexOf(BlockEndString, blockContentStartIndex);

            if (blockContentEndIndex == -1)
            {
                throw new Exception("Error uglifying: Cannot find matching end marker for start marker");
            }

            var endOfBlockEndMarkerIndex = blockContentEndIndex + BlockEndString.Length;

            return new BlockIndices
            {
                StartOfBlockStartMarkerIndex = startOfBlockStartMarkerIndex,
                BlockContentStartIndex = blockContentStartIndex,
                BlockContentEndIndex = blockContentEndIndex,
                EndOfBlockEndMarkerIndex = endOfBlockEndMarkerIndex
            };
        }

        private static string ReadStringBetween(string str, int startIndex, int endIndex)
        {
            return str.Substring(startIndex, endIndex - startIndex);
        }

        private static string PrettifyString(string uglyString)
        {
            var prettyierString = uglyString;

            prettyierString = prettyierString.Replace("\\r", "[[CRLF]]");
            prettyierString = prettyierString.Replace("\\n", "\n");
            prettyierString = prettyierString.Replace("\\t", "\t");

            return prettyierString;
        }

        private static string UglifyString(string prettyString)
        {
            var uglierString = prettyString;

            uglierString = uglierString.Replace("[[CRLF]]", "\\r");
            uglierString = uglierString.Replace("\r", "\\r");
            uglierString = uglierString.Replace("\n", "\\n");
            uglierString = uglierString.Replace("\t", "\\t");

            return uglierString;
        }

        private class BlockIndices
        {
            public int StartOfBlockStartMarkerIndex { get; set; }
            public int BlockContentStartIndex { get; set; }
            public int BlockContentEndIndex { get; set; }
            public int EndOfBlockEndMarkerIndex { get; set; }
        }
    }
}
