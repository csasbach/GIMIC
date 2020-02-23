using Gimic.Interfaces;
using Gimic.Models;
using System;
using System.IO;
using System.Linq;

namespace Gimic.Extensions
{
    public static class StreamReaderExtensions
    {
        /// <summary>
        /// Advances the stream reader to evaluate and parse the next line in the stream.
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="map"></param>
        public static void ParseNext(this StreamReader sr, IMap<string, dynamic> map)
        {
            if (sr is null || map is null) return;
            if (sr.EndOfStream) return;
            sr.EvaluateNextLine().ParseLine(sr, map);
        }
        public static void ParseMap(this StreamReader sr, IMap<string, dynamic> map, Line line, Line lookAheadLine)
        {
            if (sr is null || map is null || line is null || lookAheadLine is null) return;
            var innerMap = new Map<string, dynamic>(map);
            map.Add(line.Key, innerMap);
            lookAheadLine.ParseLine(sr, innerMap);
        }

        /// <summary>
        /// Reads and analyzes the line and returns a Line object
        /// </summary>
        /// <param name="sr"></param>
        /// <returns></returns>
        public static Line EvaluateNextLine(this StreamReader sr)
        {
            if (sr is null) return null;
            var line = sr.ReadLine();
            var key = line.GetKey();
            var tabCount = line.GetIndentTabCount();
            var value = line.GetValue(key);
            return new Line(key.Trim(), value, tabCount);
        }

        /// <summary>
        /// Tokenizes escaped sections of the value and stores captured tokens in a TokenState object
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="line"></param>
        /// <param name="tokenState"></param>
        /// <returns></returns>
        public static string CaptureAndEscapeTokens(this StreamReader sr, string line, ref TokenState tokenState)
        {
            if (sr is null || line is null || tokenState is null) return string.Empty;
            var inMiddleOfPartialToken = !string.IsNullOrEmpty(tokenState.PartialToken);
            var myValue = string.Empty;

            if (!line.Contains("``"))
            {
                if (inMiddleOfPartialToken)
                {
                    tokenState.PartialToken += line;
                    sr.AdvanceToNewLineUntilPartialTokenEnds(ref tokenState, ref myValue);
                    return myValue;
                }
                else
                {
                    return line;
                }
            }

            var chunked = line.Split(new[] { "``" }, StringSplitOptions.None).ToList();

            if (inMiddleOfPartialToken)
            {
                myValue = tokenState.CaptureAndEscapeThePartialToken(myValue, chunked);
            }

            bool lineHasTrailingPartialToken = chunked.Count % 2 == 0;

            if (lineHasTrailingPartialToken)
            {
                tokenState.StartPartialToken(chunked);
            }

            myValue = tokenState.CaptureAndEscapeOddIndexedChunks(myValue, chunked);

            if (lineHasTrailingPartialToken)
            {
                sr.AdvanceToNewLineUntilPartialTokenEnds(ref tokenState, ref myValue);
            }

            return myValue;
        }

        /// <summary>
        /// Advances the stream reader until the closing delimiter of a multi-line token has been discovered.
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="tokenState"></param>
        /// <param name="myValue"></param>
        public static void AdvanceToNewLineUntilPartialTokenEnds(this StreamReader sr, ref TokenState tokenState, ref string myValue)
        {
            if (sr is null || tokenState is null || myValue is null) return;
            tokenState.PartialToken += "\n";
            myValue += sr.CaptureAndEscapeTokens(sr.ReadLine(), ref tokenState);
        }
    }
}
