using System;
using System.Collections.Generic;

namespace AODamageMeter
{
    public enum DamageType
    {
        Unrecognized, // This shouldn't be used right now, but I want it here in case AO adds a new damage type.

        Chemical,
        Cold,
        Energy,
        Fire,
        Melee,
        Poison,
        Projectile,
        Radiation,
        Unknown, // This is an actual damage type, unlike Unrecognized.

        AimedShot,
        Brawl,
        Backstab,
        Burst,
        Dimach,
        FastAttack,
        FlingShot,
        FullAuto,
        SneakAttack,

        Reflect,
        Shield,
        Environment
    }

    public static class DamageTypeHelpers
    {
        private static IReadOnlyDictionary<string, DamageType> _damageTypes = new Dictionary<string, DamageType>(StringComparer.OrdinalIgnoreCase)
        {
            ["Chemical"] = DamageType.Chemical,
            ["Cold"] = DamageType.Cold,
            ["Energy"] = DamageType.Energy,
            ["Fire"] = DamageType.Fire,
            ["Melee"] = DamageType.Melee,
            ["Poison"] = DamageType.Poison,
            ["Projectile"] = DamageType.Projectile,
            ["Radiation"] = DamageType.Radiation,
            ["Unknown"] = DamageType.Unknown,

            ["Aimed Shot"] = DamageType.AimedShot,
            ["AimedShot"] = DamageType.AimedShot,
            ["Brawl"] = DamageType.Brawl,
            ["Brawling"] = DamageType.Brawl,
            ["Backstab"] = DamageType.Backstab,
            ["Burst"] = DamageType.Burst,
            ["Dimach"] = DamageType.Dimach,
            ["Fast Attack"] = DamageType.FastAttack,
            ["FastAttack"] = DamageType.FastAttack,
            ["Fling Shot"] = DamageType.FlingShot,
            ["FlingShot"] = DamageType.FlingShot,
            ["Full Auto"] = DamageType.FullAuto,
            ["FullAuto"] = DamageType.FullAuto,
            ["Sneak Atck"] = DamageType.SneakAttack,
            ["SneakAtck"] = DamageType.SneakAttack,
            ["Sneak Attack"] = DamageType.SneakAttack,
            ["SneakAttack"] = DamageType.SneakAttack,

            ["Reflect"] = DamageType.Reflect,
            ["Shield"] = DamageType.Shield,
            ["Environment"] = DamageType.Environment
        };

        public static DamageType GetDamageType(string value)
        {
            if (_damageTypes.TryGetValue(value, out DamageType damageType))
                return damageType;

            return DamageType.Unrecognized;
        }
    }
}
