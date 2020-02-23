using Gimic.Interfaces;
using Gimic.Models;
using System;
using System.IO;

namespace Gimic.Extensions
{
    public static class LineExtensions
    {
        /// <summary>
        /// Sets the tab count just 1 more than the current nesting level
        /// </summary>
        /// <param name="line"></param>
        /// <param name="map"></param>
        public static void NormalizeIndentRelativeTo(this Line line, IMap map)
        {
            if (line is null || map is null) return;
            line.TabCount = map.NestingLevel + 1;
        }

        /// <summary>
        /// Sets the tab count to just 1 less than the current nesting level
        /// </summary>
        /// <param name="line"></param>
        /// <param name="map"></param>
        public static void NormalizeOutdentRelativeTo(this Line line, IMap map)
        {
            if (line is null || map is null) return;
            line.TabCount = Math.Max(map.NestingLevel - 1, 0);
        }

        /// <summary>
        /// Handles the parsing of the current line of the GIMIC stream
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sr"></param>
        /// <param name="map"></param>
        public static void ParseLine(this Line line, StreamReader sr, IMap<string, dynamic> map)
        {
            if (line is null || sr is null || map is null) return;
            line = line.SkipLinesWithNoKey(sr);
            if (string.IsNullOrWhiteSpace(line.Key) && sr.EndOfStream) return;

            map = map.CloseIfComplete(line);

            if (line.IsMapValue(map, sr, out var lookAheadLine))
            {
                sr.ParseMap(map, line, lookAheadLine);
            }
            else
            {
                line.ParseValue(sr, map);
                lookAheadLine.ParseLine(sr, map);
            }

            sr.ParseNext(map);
        }

        /// <summary>
        /// Parses the value from a line of GIMIC text
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sr"></param>
        /// <param name="map"></param>
        public static void ParseValue(this Line line, StreamReader sr, IMap<string, dynamic> map)
        {
            if (line is null || sr is null || map is null) return;
            var tokenState = new TokenState();
            dynamic value = sr.CaptureAndEscapeTokens((string)line.Value, ref tokenState);

            value = value.Contains(',')
                    ? tokenState.CreateListOfValues((string)value)
                    : tokenState.ReInsertCapturedTokens((string)value);

            map.Add(line.Key, value);
        }

        /// <summary>
        /// Extracts the key from a line of GIMIC text
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetKey(this string line)
        {
            if (line is null) return string.Empty;
            return line.Substring(0, Math.Max(line.IndexOf(':'), 0)).TrimEnd(':');
        }

        /// <summary>
        /// Extracts the value from a line of GIMIC text
        /// </summary>
        /// <param name="line"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(this string line, string key)
        {
            if (line is null || key is null) return string.Empty;
            return string.IsNullOrEmpty(line) ? line : line.Remove(0, key.Length + 1);
        }

        /// <summary>
        /// Counts the number of indent tabs at the front of a line of GIMIC text
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static int GetIndentTabCount(this string line)
        {
            if (line is null) return 0;
            var tabCount = 0;
            foreach (var ch in line)
            {
                if (ch != '\t') break;
                tabCount++;
            }
            return tabCount;
        }

        /// <summary>
        /// Advances the parser to the next line that contains a valid GIMIC key
        /// </summary>
        /// <param name="line"></param>
        /// <param name="sr"></param>
        /// <returns></returns>
        public static Line SkipLinesWithNoKey(this Line line, StreamReader sr)
        {
            if (line is null || sr is null) return null;
            while (string.IsNullOrWhiteSpace(line?.Key) && !sr.EndOfStream) line = sr.EvaluateNextLine();
            return line;
        }

        /// <summary>
        /// Determines if the tab count of the next line confirms the beginning of a map value
        /// or if this is just a key with no value.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="map"></param>
        /// <param name="sr"></param>
        /// <param name="lookAheadLine"></param>
        /// <returns></returns>
        public static bool IsMapValue(this Line line, IMap map, StreamReader sr, out Line lookAheadLine)
        {
            lookAheadLine = null;
            if (line is null || map is null || sr is null) return false;
            if (string.IsNullOrWhiteSpace(line.Value) && !sr.EndOfStream)
            {
                lookAheadLine = sr.EvaluateNextLine();
                if (lookAheadLine.TabCount > map.NestingLevel)
                {
                    lookAheadLine.NormalizeIndentRelativeTo(map);
                    return true;
                }
            }
            return false;
        }
    }

}
