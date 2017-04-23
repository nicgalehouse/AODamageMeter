using System;
using System.Collections.Generic;

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
    }
}
