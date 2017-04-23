using System;
using System.Collections.Generic;

namespace AODamageMeter
{
    public enum DamageType
    {
        Chemical,
        Energy,
        Melee,
        Poison,
        Projectile,
        Radiation,

        Burst,

        Reflect,
        Shield,
        Environment
    }

    public static class DamageTypeHelpers
    {
        // For performance(?), use a dictionary rather than Enum.Parse's reflection.
        private static IReadOnlyDictionary<string, DamageType> _damageTypes = new Dictionary<string, DamageType>(StringComparer.OrdinalIgnoreCase)
        {
            ["Chemical"] = DamageType.Chemical,
            ["Energy"] = DamageType.Energy,
            ["Melee"] = DamageType.Melee,
            ["Poison"] = DamageType.Poison,
            ["Projectile"] = DamageType.Projectile,
            ["Radiation"] = DamageType.Radiation,

            ["Burst"] = DamageType.Burst,

            ["Reflect"] = DamageType.Reflect,
            ["Shield"] = DamageType.Shield,
            ["Environment"] = DamageType.Environment
        };

        public static DamageType GetDamageType(string value)
            => _damageTypes[value];
    }
}
