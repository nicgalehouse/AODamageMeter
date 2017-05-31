namespace AODamageMeter.UI.Helpers
{
    public static class NumberFormatter
    {
        public static string Format(this int n)
            => Format((long)n);

        public static string Format(this long n)
            => n < 100 ? n.ToString() : Format((double)n);

        public static string Format(this double n)
        {
            if (n == 0) return "0";
            if (n < 100) return n.ToString("F1");                          // 50.4
            if (n < 1000) return n.ToString("F0");                         // 836
            if (n < 10000) return $"{(n / 1000).ToString("F2")}K";         // 9.42K
            if (n < 1000000) return $"{(n / 1000).ToString("F1")}K";       // 12.3K, 432.4K
            if (n < 1000000000) return $"{(n / 1000000).ToString("F2")}M"; // 1.05M, 949.12M
            else return $"{(n / 1000000000).ToString("F3")}B";             // 1.344B
        }

        public static string FormatPercent(this double n)
        {
            if (n == 1) return "100%";
            return $"{(100 * n).ToString("F1")}%";
        }
    }
}
