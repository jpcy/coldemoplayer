using System;
using System.Linq;
using System.Text;

namespace CDP.Core.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveChars(this string s, params char[] chars)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                if (!chars.Contains(s[i]))
                {
                    sb.Append(s[i]);
                }
            }

            return sb.ToString();
        }
    }
}
