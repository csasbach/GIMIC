using Gimic.Models;
using System.Collections.Generic;
using System.IO;

namespace Gimic.Extensions
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Parses a stream of GIMIC text into a Dictionary
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Dictionary<string, dynamic> ParseGimic(this Stream stream)
        {
            using var sr = new StreamReader(stream);
            var retVal = new Map<string, dynamic>(null);
            sr.ParseNext(retVal);
            return retVal;
        }
    }
}
