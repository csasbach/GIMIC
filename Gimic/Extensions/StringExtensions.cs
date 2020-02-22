using System.IO;

namespace Gimic.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string into a memory stream.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Stream GenerateStreamFromString(this string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
