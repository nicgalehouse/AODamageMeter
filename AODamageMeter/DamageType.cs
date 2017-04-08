using System;
using System.Collections.Generic;

namespace AODamageMeter
{
    public enum DamageType
    {
        Melee,
        Projectile,
        Reflect,
        Shield,
    }

    public static class DamageTypeHelpers
    {
        // For performance, use a dictionary rather than Enum.Parse's reflection.
        private static IReadOnlyDictionary<string, DamageType> _damageTypes = new Dictionary<string, DamageType>(StringComparer.OrdinalIgnoreCase)
        {
            ["Melee"] = DamageType.Melee,
            ["Projectile"] = DamageType.Projectile,
            ["Reflect"] = DamageType.Reflect,
            ["Shield"] = DamageType.Shield
        };

        public static DamageType GetDamageType(string value)
            => _damageTypes[value];
    }
}
