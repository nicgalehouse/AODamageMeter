using System.Collections.ObjectModel;

namespace AODamageMeter.UI.Extensions
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
    }
}
