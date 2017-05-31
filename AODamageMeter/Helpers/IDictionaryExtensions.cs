using System.Collections.Generic;

namespace AODamageMeter.Helpers
{
    public static class IDictionaryExtensions
    {
        public static void Increment<TKey>(this IDictionary<TKey, int> dictionary, TKey key, int increment)
        {
            dictionary.TryGetValue(key, out int existingValueOrZero);
            dictionary[key] = existingValueOrZero + increment;
        }

        public static void Increment<TKey>(this IDictionary<TKey, long> dictionary, TKey key, long increment)
        {
            dictionary.TryGetValue(key, out long existingValueOrZero);
            dictionary[key] = existingValueOrZero + increment;
        }

        public static TValue GetValueOrFallback<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue fallback = default(TValue))
            => dictionary.TryGetValue(key, out TValue value) ? value : fallback;
    }
}
