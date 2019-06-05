using System.Collections.Generic;
using System.Net;

namespace WebDavCommon.Helpers
{
    /// <summary>
    /// This class provide methods to decode and encode url strings.
    /// </summary>
    public static class UrlHelper
    {
        /// <summary>Decodes url string.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The <see cref="string"/> with decoded url.</returns>
        public static string Decode(string source)
        {
            string[] sourceElements = source.Split('/');
            var resultElements = new List<string>();
            foreach (string element in sourceElements)
            {
                resultElements.Add(WebUtility.UrlDecode(element));
            }

            string[] resultArray = resultElements.ToArray();
            return string.Join("/", resultArray);
        }

        /// <summary>Encodes url string</summary>
        /// <param name="source">The source.</param>
        /// <returns>The <see cref="string"/> with encoded url.</returns>
        public static string Encode(string source)
        {
            string[] sourceElements = source.Split('/');
            var resultElements = new List<string>();
            foreach (string element in sourceElements)
            {
                resultElements.Add(WebUtility.UrlEncode(element));
            }

            string[] resultArray = resultElements.ToArray();
            return string.Join("/", resultArray);
        }
    }
}
