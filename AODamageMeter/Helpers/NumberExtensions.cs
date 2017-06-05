namespace AODamageMeter.Helpers
{
    public static class NumberExtensions
    {
        public static double? NullIfZero(this int value) => value == 0 ? (double?)null : value;
        public static double? NullIfZero(this long value) => value == 0 ? (double?)null : value;
        public static double? NullIfZero(this double value) => value == 0 ? (double?)null : value;

        public static double? NullIfZero(this int? value) => value == 0 ? null : value;
        public static double? NullIfZero(this long? value) => value == 0 ? null : value;
        public static double? NullIfZero(this double? value) => value == 0 ? null : value;
    }
}
