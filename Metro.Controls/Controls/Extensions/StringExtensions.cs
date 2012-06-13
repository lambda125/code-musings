using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metro.Controls.Extensions
{
    public static class StringExtensions
    {
        public static bool IsEmpty(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return true;

            s = s.Trim();

            if (string.IsNullOrWhiteSpace(s))
                return true;

            return false;
        }

        public static bool IsNotEmpty(this string s)
        {
            return !s.IsEmpty();
        }

        public static bool ContainsIgnoringCase(this string thisString, string otherString)
        {
            if (thisString.IsEmpty() || otherString.IsEmpty())
                return false;

            var thisAsLower = thisString.ToLowerInvariant();
            var otherAsLower = otherString.ToLowerInvariant();

            return thisAsLower.Contains(otherAsLower);
        }
    }
}
