using System.Windows;

namespace AODamageMeter.UI.Helpers
{
    public static class FreezableHelper
    {
        public static T Frozen<T>(this T freezable) where T : Freezable
        {
            if (freezable.CanFreeze && !freezable.IsFrozen)
            {
                freezable.Freeze();
            }

            return freezable;
        }
    }
}
