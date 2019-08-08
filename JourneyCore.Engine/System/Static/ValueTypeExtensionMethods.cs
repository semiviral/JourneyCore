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
            string _encodedBase64String = rawBase64.Trim('=').Replace('+', '-').Replace('/', '_');
            int _paddingCount = rawBase64.Count(character => character == '=');

            return $"{_encodedBase64String}{_paddingCount}";
        }

        public static string HtmlDecodeBase64(this string encodedBase64)
        {
            string _decodedBase64String =
                encodedBase64.Replace('-', '+').Replace('_', '/').Substring(0, encodedBase64.Length - 1);

            int.TryParse(encodedBase64.Substring(encodedBase64.Length - 1), out int _paddingCount);
            string _paddingCharacters = new string('=', _paddingCount);

            return $"{_decodedBase64String}{_paddingCharacters}";
        }
        
        public static Vector2f Sum(this IEnumerable<Vector2f> vectors)
        {
            Vector2f _collapsed = new Vector2f(0f, 0f);

            return vectors.Aggregate(_collapsed, (current, vector) => current + vector);
        }
    }
}