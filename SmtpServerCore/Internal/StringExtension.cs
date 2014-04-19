using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Toolbelt.Net.Smtp.Internal
{
    internal static class StringExtension
    {
        public static string[] SplitAndTrim(this string text, params char[] separators)
        {
            return text.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(_ => _.Trim())
                .Where(_ => _.IsNotNullOrEmpty())
                .ToArray();
        }

        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool IsNotNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text) == false;
        }

        public static string ToBase64(this IEnumerable<byte> bytes)
        {
            return Convert.ToBase64String(bytes.ToArray());
        }

        public static byte[] Base64ToBytes(this string text)
        {
            return Convert.FromBase64String(text);
        }

        public static byte[] GetBytes(this string text, Encoding encoding = null)
        {
            return (encoding ?? Encoding.UTF8).GetBytes(text);
        }

        public static string ToString(this IEnumerable<byte> bytes, Encoding encoding)
        {
            return encoding.GetString(bytes.ToArray());
        }

        public static string Combine(this IEnumerable<string> texts, string separator = "")
        {
            return string.Join(separator, texts);
        }

        public static string ToHexString(this IEnumerable<byte> bytes)
        {
            return BitConverter.ToString(bytes.ToArray()).Replace("-", "").ToLower();
        }
    }
}
