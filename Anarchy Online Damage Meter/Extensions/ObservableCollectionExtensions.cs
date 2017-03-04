using System;
using System.Collections.ObjectModel;

namespace Anarchy_Online_Damage_Meter.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static void RemoveAll<T>(this ObservableCollection<T> collection)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                    collection.RemoveAt(i);
            }
        }

        public static void RemoveAll<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
        {
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (condition(collection[i]))
                {
                    collection.RemoveAt(i);
                }
            }
        }
    }
}
