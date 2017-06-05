using System;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter.Helpers
{
    internal static class IEnumerableExtensions
    {
        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var element in source)
            {
                action(element);
            }
        }

        public static int? NullableSum<T>(this IEnumerable<T> source, Func<T, int?> selector)
            => source.Select(selector).Any(i => i.HasValue) ? source.Sum(selector) : null;

        public static long? NullableSum<T>(this IEnumerable<T> source, Func<T, long?> selector)
            => source.Select(selector).Any(i => i.HasValue) ? source.Sum(selector) : null;

        public static double? NullableSum<T>(this IEnumerable<T> source, Func<T, double?> selector)
            => source.Select(selector).Any(i => i.HasValue) ? source.Sum(selector) : null;

        public static int? NullableMax<T>(this IEnumerable<T> source, Func<T, int> selector)
            => source.Any() ? source.Max(selector) : (int?)null;

        public static long? NullableMax<T>(this IEnumerable<T> source, Func<T, long> selector)
            => source.Any() ? source.Max(selector) : (long?)null;

        public static double? NullableMax<T>(this IEnumerable<T> source, Func<T, double> selector)
            => source.Any() ? source.Max(selector) : (double?)null;
    }
}
