using AODamageMeter.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter
{
    public class FightDamageTakenStats
    {
        public FightDamageTakenStats(Fight fight, bool includeNPCs = true, bool includeZeroDamageTakens = true)
        {
            Fight = fight;
            FightCharacters = fight.FightCharacters
                .Where(c => (includeNPCs || !c.IsNPC)
                    && (includeZeroDamageTakens || c.TotalDamageTaken != 0))
                .ToArray();

            foreach (var fightCharacter in FightCharacters)
            {
                WeaponDamage += fightCharacter.WeaponDamageTaken;
                CritDamage += fightCharacter.CritDamageTaken;
                GlanceDamage += fightCharacter.GlanceDamageTaken;
                NanoDamage += fightCharacter.NanoDamageTaken;
                IndirectDamage += fightCharacter.IndirectDamageTaken;

                WeaponHits += fightCharacter.WeaponHitsTaken;
                Crits += fightCharacter.CritsTaken;
                Glances += fightCharacter.GlancesTaken;
                Misses += fightCharacter.MissesTaken;
                NanoHits += fightCharacter.NanoHitsTaken;
                IndirectHits += fightCharacter.IndirectHitsTaken;
            }
        }

        public Fight Fight { get; }
        public IReadOnlyList<FightCharacter> FightCharacters { get; }
        public int FightCharacterCount => FightCharacters.Count;
        public long WeaponDamage { get; }
        public long CritDamage { get; }
        public long GlanceDamage { get; }
        public long NanoDamage { get; }
        public long IndirectDamage { get; }
        public long TotalDamage => WeaponDamage + NanoDamage + IndirectDamage;

        public double? WeaponDamagePM => WeaponDamage / Fight.Duration?.TotalMinutes;
        public double? NanoDamagePM => NanoDamage / Fight.Duration?.TotalMinutes;
        public double? IndirectDamagePM => IndirectDamage / Fight.Duration?.TotalMinutes;
        public double? TotalDamagePM => TotalDamage / Fight.Duration?.TotalMinutes;

        public double? WeaponPercentOfTotalDamage => WeaponDamage / TotalDamage.NullIfZero();
        public double? NanoPercentOfTotalDamage => NanoDamage / TotalDamage.NullIfZero();
        public double? IndirectPercentOfTotalDamage => IndirectDamage / TotalDamage.NullIfZero();

        public int WeaponHits { get; }
        public int Crits { get; }
        public int Glances { get; }
        public int Misses { get; }
        public int WeaponHitAttempts => WeaponHits + Misses;
        public int NanoHits { get; }
        public int IndirectHits { get; }
        public int TotalHits => WeaponHits + NanoHits + IndirectHits;

        public double? WeaponHitsPM => WeaponHits / Fight.Duration?.TotalMinutes;
        public double? CritsPM => Crits / Fight.Duration?.TotalMinutes;
        public double? GlancesPM => Glances / Fight.Duration?.TotalMinutes;
        public double? MissesPM => Misses / Fight.Duration?.TotalMinutes;
        public double? WeaponHitAttemptsPM => WeaponHitAttempts / Fight.Duration?.TotalMinutes;
        public double? NanoHitsPM => NanoHits / Fight.Duration?.TotalMinutes;
        public double? IndirectHitsPM => IndirectHits / Fight.Duration?.TotalMinutes;
        public double? TotalHitsPM => TotalHits / Fight.Duration?.TotalMinutes;

        public double? WeaponHitChance => WeaponHits / WeaponHitAttempts.NullIfZero();
        public double? CritChance => Crits / WeaponHits.NullIfZero();
        public double? GlanceChance => Glances / WeaponHits.NullIfZero();
        public double? MissChance => Misses / WeaponHitAttempts.NullIfZero();

        public double? AverageWeaponDamage => WeaponDamage / WeaponHits.NullIfZero();
        public double? AverageCritDamage => CritDamage / Crits.NullIfZero();
        public double? AverageGlanceDamage => GlanceDamage / Glances.NullIfZero();
        public double? AverageNanoDamage => NanoDamage / NanoHits.NullIfZero();
        public double? AverageIndirectDamage => IndirectDamage / IndirectHits.NullIfZero();

        public bool HasDamageTypeDamage(DamageType damageType) => FightCharacters.Any(c => c.DamageTypeDamagesTaken.ContainsKey(damageType));
        public bool HasSpecials => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamage);
        public int? GetDamageTypeHits(DamageType damageType) => FightCharacters.NullableSum(c => c.GetDamageTypeHitsTaken(damageType));
        public long? GetDamageTypeDamage(DamageType damageType) => FightCharacters.NullableSum(c => c.GetDamageTypeDamageTaken(damageType));
        public double? GetAverageDamageTypeDamage(DamageType damageType) => GetDamageTypeDamage(damageType) / (double?)GetDamageTypeHits(damageType);
        public double? GetSecondsPerDamageTypeHit(DamageType damageType) => Fight.Duration?.TotalSeconds / GetDamageTypeHits(damageType);
    }
}
