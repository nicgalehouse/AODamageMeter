using System;

namespace AODamageMeter.Helpers
{
    public class NumberFormatter
    {
        public static string MakePretty(double x)
        {
            if (x > 100000)
            {
                
            }

            return "hi";
        }

        public static string DynamicPrettifier<T>(T x)
        {
            return "x";
        }

        public static string InThousands(double x)
            => Math.Round((x / 1000), 0).ToString() + "k";

        public static string InThousands(int x)
            => Math.Round(((double)x / 1000), 0).ToString() + "k";

        public static string InThousands(long x)
            => Math.Round(((double)x / 1000), 0).ToString() + "k";

        public static string ThousandsSeparator(double x)
            => Math.Round(x,0).ToString("#,##");

        public static string ThousandsSeparator(int x)
            => x.ToString("#,##");

        public static string ThousandsSeparator(long x)
            => x.ToString("#,##");

    }
}
