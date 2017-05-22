using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AODamageMeter
{
    public enum DamageType
    {
        Unrecognized,

        Chemical,
        Cold,
        Energy,
        Fire,
        Melee,
        Poison,
        Projectile,
        Radiation,
        Nano,
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
            ["Nano"] = DamageType.Nano,
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

#if DEBUG
            Debug.WriteLine($"Unrecognized damage type: {value}");
#endif

            return DamageType.Unrecognized;
        }

        public static string GetName(this DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Unrecognized: return "Unrecognized";

                case DamageType.Chemical: return "Chemical";
                case DamageType.Cold: return "Cold";
                case DamageType.Energy: return "Energy";
                case DamageType.Fire: return "Fire";
                case DamageType.Melee: return "Melee";
                case DamageType.Poison: return "Poison";
                case DamageType.Projectile: return "Projectile";
                case DamageType.Radiation: return "Radiation";
                case DamageType.Nano: return "Nano";
                case DamageType.Unknown: return "Unknown";

                case DamageType.AimedShot: return "Aimed Shot";
                case DamageType.Brawl: return "Brawl";
                case DamageType.Backstab: return "Backstab";
                case DamageType.Burst: return "Burst";
                case DamageType.Dimach: return "Dimach";
                case DamageType.FastAttack: return "Fast Attack";
                case DamageType.FlingShot: return "Fling Shot";
                case DamageType.FullAuto: return "Full Auto";
                case DamageType.SneakAttack: return "Sneak Attack";

                case DamageType.Reflect: return "Reflect";
                case DamageType.Shield: return "Shield";
                case DamageType.Environment: return "Environment";

                default: throw new NotImplementedException();
            }
        }

        public static readonly IReadOnlyList<DamageType> AllDamageTypes = Enum.GetValues(typeof(DamageType))
            .Cast<DamageType>()
            .OrderBy(t => t)
            .ToArray();

        public static readonly IReadOnlyList<DamageType> SpecialDamageTypes = new[]
        {
            DamageType.AimedShot, DamageType.Brawl, DamageType.Backstab, DamageType.Burst, DamageType.Dimach,
            DamageType.FastAttack, DamageType.FlingShot, DamageType.FullAuto, DamageType.SneakAttack
        };
    }
}
