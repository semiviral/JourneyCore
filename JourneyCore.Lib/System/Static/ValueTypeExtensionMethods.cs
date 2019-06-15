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
            return rawBase64.Trim('=').Replace('+', '-').Replace('/', '_');
        }

        public static string HtmlDecodeBase64(this string encodedBase64)
        {
            return encodedBase64.Insert(encodedBase64.Length, "=").Replace('-', '+').Replace('_', '/');
        }
    }
}