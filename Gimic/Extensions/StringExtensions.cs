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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposing this makes the stream unreadable.")]
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
