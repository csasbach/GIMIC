using Gimic.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gimic.Extensions
{
    public static class TokenStateExtensions
    {
        /// <summary>
        /// Splits a comma delimited GIMIC value into a list of string values
        /// </summary>
        /// <param name="tokenState"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static dynamic CreateListOfValues(this TokenState tokenState, dynamic value)
        {
            var list = new List<string>();
            var chunked = value.Split(',');
            foreach (var chunk in chunked)
            {
                list.Add(tokenState.ReInsertCapturedTokens((string)chunk));
            }
            value = list;
            return value;
        }

        /// <summary>
        /// Replaces a captured token with a key value that can be replaced later
        /// </summary>
        /// <param name="tokenState"></param>
        /// <param name="myValue"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static string CaptureAndEscapeToken(this TokenState tokenState, string myValue, string token)
        {
            if (tokenState is null || myValue is null || token is null) return string.Empty;
            var tokenId = Guid.NewGuid().ToString();
            myValue += tokenId;
            tokenState.CapturedTokens.Add((token, tokenId));
            return myValue;
        }

        /// <summary>
        /// Escapes tokens between `` delimiters and stores them in the TokenState object
        /// </summary>
        /// <param name="tokenState"></param>
        /// <param name="myValue"></param>
        /// <param name="chunked"></param>
        /// <returns></returns>
        public static string CaptureAndEscapeOddIndexedChunks(this TokenState tokenState, string myValue, List<string> chunked)
        {
            if (tokenState is null || myValue is null || chunked is null) return string.Empty;
            var chunkIndex = 0;
            foreach (var chunk in chunked)
            {
                if (chunkIndex % 2 == 0)
                {
                    myValue += chunk;
                }
                else
                {
                    myValue = tokenState.CaptureAndEscapeToken(myValue, chunk);
                }
                chunkIndex++;
            }
            return myValue;
        }

        /// <summary>
        /// Captures the beginning fragment of a multi-line escaped token
        /// </summary>
        /// <param name="tokenState"></param>
        /// <param name="chunked"></param>
        public static void StartPartialToken(this TokenState tokenState, List<string> chunked)
        {
            if (tokenState is null || chunked is null) return;
            var last = chunked.Last();
            chunked.Remove(last);

            tokenState.PartialToken += last;
        }

        /// <summary>
        /// Completes the escaping of a multi-line token and stores it in the TokenState
        /// </summary>
        /// <param name="tokenState"></param>
        /// <param name="myValue"></param>
        /// <param name="chunked"></param>
        /// <returns></returns>
        public static string CaptureAndEscapeThePartialToken(this TokenState tokenState, string myValue, List<string> chunked)
        {
            if (tokenState is null || myValue is null || chunked is null) return string.Empty;
            var first = chunked.First();
            chunked.Remove(first);

            tokenState.PartialToken += first;
            myValue += tokenState.CaptureAndEscapeToken(myValue, tokenState.PartialToken);
            tokenState.PartialToken = string.Empty;

            return myValue;
        }

        /// <summary>
        /// Re-inserts the original token values back into the tokenized GIMIC value
        /// </summary>
        /// <param name="tokenState"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ReInsertCapturedTokens(this TokenState tokenState, string value)
        {
            if (tokenState is null || value is null) return string.Empty;
            foreach (var token in tokenState.CapturedTokens)
            {
                value = value?.Replace(token.id, @$"{token.token}");
            }
            return value?.Trim();
        }
    }
}
