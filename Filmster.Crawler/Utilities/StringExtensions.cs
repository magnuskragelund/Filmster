using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Filmster.Utilities
{
    public static class StringExtensions
    {
        /// <summary>
        /// Shortens a string by a certain character to prevent it being cut off in the middle of a word or sentence.
        /// </summary>
        /// <param name="text">The string to shorten</param>
        /// <param name="truncateTo">The character(s) to cut the string off by</param>
        /// <param name="maxLength">The maximum lenth of the retuned string</param>
        /// <param name="isTruncated">Indicates if the string were shortened</param>
        /// <returns>A string no longer than the max length, potentially cut off by a given character</returns> 
        public static string ShortenString(this string text, string truncateTo, int maxLength, out bool isTruncated)
        {
            isTruncated = (text.Length > maxLength);
            if(isTruncated)
            {
                text = text.Substring(0, Math.Min(text.Length, maxLength));
                int i = text.LastIndexOf(truncateTo);
                if (i > -1)
                {
                    text = text.Substring(0, i);
                }
                return text;
            }
            return text;
        }

        public static string ShortenString(this string text, string truncateTo, int maxLength)
        {
            bool isTrunated;
            return text.ShortenString(truncateTo, maxLength, out isTrunated);
        }


        /// <summary>
        /// Replaces a list of values in the specified string.
        /// </summary>
        /// <param name="text">The string to replace in</param>
        /// <param name="replaceValues">A dictionary of keys and values to replace</param>
        /// <returns>A string with replaced values</returns>
        public static string ReplaceMany(this string text, Dictionary<string, string> replaceValues)
        {
            foreach(KeyValuePair<string, string> replaceValue in replaceValues)
            {
                text = text.Replace(replaceValue.Key, replaceValue.Value);
            }
            return text;
        }

        /// <summary>
        /// Returns the string between the two specified indexes
        /// </summary>
        /// <param name="text">The string to perform the substring on</param>
        /// <param name="startIndex">Index from where the substring should start</param>
        /// <param name="endIndex">Index to where the substring should end</param>
        /// <returns></returns>
        public static string SubstringByIndexToIndex(this string text, int startIndex, int endIndex)
        {
            return text.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Returns the string between the two specified tokens
        /// </summary>
        /// <param name="text">The string to perform the substring on</param>
        /// <param name="startToken">String token from where the substring should start</param>
        /// <param name="endToken">String token to where the substring should end</param>
        /// <param name="includeTokens">Indicates if the tokens should be included in the returned string</param>
        /// <returns></returns>
        public static string SubstringByStringToString(this string text, string startToken, string endToken, bool includeTokens)
        {
            int startIndex = text.IndexOf(startToken);
            int endIndex = text.IndexOf(endToken, startIndex + startToken.Length);

            if(startIndex == -1)
            {
                throw new ArgumentException(String.Format("startToken: '{0}' not found in string", startToken));
            }

            if (endIndex == -1)
            {
                throw new ArgumentException(String.Format("endToken: '{0}' not found in string", endToken));
            }

            if (includeTokens)
            {
                endIndex += endToken.Length;
            }
            else
            {
                startIndex += startToken.Length;
            }

            return text.Substring(startIndex, endIndex - startIndex);
        }

        public static string TrySubstringByStringToString(this string text, string startToken, string endToken, bool includeTokens)
        {
            try
            {
                return SubstringByStringToString(text, startToken, endToken, includeTokens);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Strips all html tags from the passed in string
        /// </summary>
        /// <param name="htmlString">The string to strip html tags from</param>
        /// <returns>A string stripped of html tags</returns>
        public static string StripHtml(this string htmlString)
        {
            return Regex.Replace(htmlString, @"<(.|\n)*?>", String.Empty);
        }

        public static string RemoveNonNumericChars(this string text)
        {
            return Regex.Replace(text, "\\D", "");
        }

    }
}
