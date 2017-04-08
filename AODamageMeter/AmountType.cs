using System;
using System.Collections.Generic;

namespace AODamageMeter
{
    public enum AmountType
    {
        Melee,
        Projectile
    }

    public static class AmountTypeHelpers
    {
        // For performance, use a dictionary rather than Enum.Parse's reflection.
        private static IReadOnlyDictionary<string, AmountType> _amountTypes = new Dictionary<string, AmountType>(StringComparer.OrdinalIgnoreCase)
        {
            ["Melee"] = AmountType.Melee,
            ["Projectile"] = AmountType.Projectile
        };

        public static AmountType GetAmountType(this string value)
            => _amountTypes[value];
    }
}
