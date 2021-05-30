using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ZTEModem
{
    public static class ExtendedFunction
    {
        public static List<string> Matches(this string text, string query, string @default = "")
        {
            var result = new List<string>();

            var regex = Regex.Match(text, query);
            if (regex.Success)
            {
                for (int i = 1; i <= regex.Groups.Count; i++)
                    result.Add(regex.Groups[i].Value);
            }
            else result.Add(@default);

            return result;
        }

        public static string Find(this string text, string query, string @default = "")
        {
            var regex = Regex.Match(text, query);
            if (regex.Success) return regex.Groups[1].Value;
            else return @default;
        }

        public static string Sha256(this string text)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(text));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static string ToAsterisk(this string text)
        {
            return Regex.Replace(text, ".", "*");
        }

        public static string HtmlDecode(this string text)
        {
            try
            {
                return HttpUtility.HtmlDecode(text);
            }
            catch (Exception)
            {
                return text;
            }
        }
    }
}
