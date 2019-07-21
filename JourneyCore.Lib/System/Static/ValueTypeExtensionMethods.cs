using System.Collections.Generic;
using System.Linq;
using SFML.System;

namespace JourneyCore.Lib.System.Static
{
    public static class ValueTypeExtensionMethods
    {
        public static float LimitToRange(this float subject, float minimumRange, float maximumRange)
        {
            if (subject > maximumRange)
            {
                return maximumRange;
            }

            if (subject < minimumRange)
            {
                return minimumRange;
            }

            return subject;
        }

        public static string HtmlEncodeBase64(this string rawBase64)
        {
            string encodedBase64String = rawBase64.Trim('=').Replace('+', '-').Replace('/', '_');
            int paddingCount = rawBase64.Count(character => character == '=');

            return $"{encodedBase64String}{paddingCount}";
        }

        public static string HtmlDecodeBase64(this string encodedBase64)
        {
            string decodedBase64String =
                encodedBase64.Replace('-', '+').Replace('_', '/').Substring(0, encodedBase64.Length - 1);

            int.TryParse(encodedBase64.Substring(encodedBase64.Length - 1), out int paddingCount);
            string paddingCharacters = new string('=', paddingCount);

            return $"{decodedBase64String}{paddingCharacters}";
        }

        public static Vector2f Sum(this IEnumerable<Vector2f> vectors)
        {
            Vector2f collapsed = new Vector2f(0f, 0f);

            return vectors.Aggregate(collapsed, (current, vector) => current + vector);
        }
    }
}