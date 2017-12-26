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
                .Where(c => (includeNPCs || c.IsPlayerOrFightPet)
                    && (includeZeroDamageTakens || c.TotalDamageTaken != 0))
                .ToArray();

            foreach (var fightCharacter in FightCharacters)
            {
                WeaponDamage += fightCharacter.WeaponDamageTaken;
                RegularDamage += fightCharacter.RegularDamageTaken;
                NormalDamage += fightCharacter.NormalDamageTaken;
                CritDamage += fightCharacter.CritDamageTaken;
                GlanceDamage += fightCharacter.GlanceDamageTaken;
                SpecialDamage += fightCharacter.SpecialDamageTaken;
                NanoDamage += fightCharacter.NanoDamageTaken;
                IndirectDamage += fightCharacter.IndirectDamageTaken;
                AbsorbedDamage += fightCharacter.AbsorbedDamageTaken;

                WeaponHits += fightCharacter.WeaponHitsTaken;
                Regulars += fightCharacter.RegularsTaken;
                Normals += fightCharacter.NormalsTaken;
                Crits += fightCharacter.CritsTaken;
                Glances += fightCharacter.GlancesTaken;
                Specials += fightCharacter.SpecialsTaken;
                Misses += fightCharacter.MissesTaken;
                NanoHits += fightCharacter.NanoHitsTaken;
                IndirectHits += fightCharacter.IndirectHitsTaken;
                AbsorbedHits += fightCharacter.AbsorbedHitsTaken;

                foreach (var damageTypeHitsTaken in fightCharacter.DamageTypeHitsTaken)
                {
                    _damageTypeHits.Increment(damageTypeHitsTaken.Key, damageTypeHitsTaken.Value);
                }
                foreach (var damageTypeDamageTaken in fightCharacter.DamageTypeDamagesTaken)
                {
                    _damageTypeDamages.Increment(damageTypeDamageTaken.Key, damageTypeDamageTaken.Value);
                }
            }
        }

        public Fight Fight { get; }
        public IReadOnlyList<FightCharacter> FightCharacters { get; }
        public int FightCharacterCount => FightCharacters.Count;
        public long WeaponDamage { get; }
        public long RegularDamage { get; }
        public long NormalDamage { get; }
        public long CritDamage { get; }
        public long GlanceDamage { get; }
        public long SpecialDamage { get; }
        public long NanoDamage { get; }
        public long IndirectDamage { get; }
        public long AbsorbedDamage { get; }
        public long TotalDamage => WeaponDamage + NanoDamage + IndirectDamage + AbsorbedDamage;

        public double? WeaponDamagePM => WeaponDamage / Fight.Duration?.TotalMinutes;
        public double? NanoDamagePM => NanoDamage / Fight.Duration?.TotalMinutes;
        public double? IndirectDamagePM => IndirectDamage / Fight.Duration?.TotalMinutes;
        public double? AbsorbedDamagePM => AbsorbedDamage / Fight.Duration?.TotalMinutes;
        public double? TotalDamagePM => TotalDamage / Fight.Duration?.TotalMinutes;

        public double? WeaponPercentOfTotalDamage => WeaponDamage / TotalDamage.NullIfZero();
        public double? NanoPercentOfTotalDamage => NanoDamage / TotalDamage.NullIfZero();
        public double? IndirectPercentOfTotalDamage => IndirectDamage / TotalDamage.NullIfZero();
        public double? AbsorbedPercentOfTotalDamage => AbsorbedDamage / TotalDamage.NullIfZero();

        public int WeaponHits { get; }
        public int Regulars { get; }
        public int Normals { get; }
        public int Crits { get; }
        public int Glances { get; }
        public int Specials { get; }
        public int Misses { get; }
        public int WeaponHitAttempts => WeaponHits + Misses;
        public int NanoHits { get; }
        public int IndirectHits { get; }
        public int AbsorbedHits { get; }
        public int TotalHits => WeaponHits + NanoHits + IndirectHits + AbsorbedHits;

        public double? WeaponHitsPM => WeaponHits / Fight.Duration?.TotalMinutes;
        public double? RegularsPM => Regulars / Fight.Duration?.TotalMinutes;
        public double? NormalsPM => Normals / Fight.Duration?.TotalMinutes;
        public double? CritsPM => Crits / Fight.Duration?.TotalMinutes;
        public double? GlancesPM => Glances / Fight.Duration?.TotalMinutes;
        public double? SpecialsPM => Specials / Fight.Duration?.TotalMinutes;
        public double? MissesPM => Misses / Fight.Duration?.TotalMinutes;
        public double? WeaponHitAttemptsPM => WeaponHitAttempts / Fight.Duration?.TotalMinutes;
        public double? NanoHitsPM => NanoHits / Fight.Duration?.TotalMinutes;
        public double? IndirectHitsPM => IndirectHits / Fight.Duration?.TotalMinutes;
        public double? AbsorbedHitsPM => AbsorbedHits / Fight.Duration?.TotalMinutes;
        public double? TotalHitsPM => TotalHits / Fight.Duration?.TotalMinutes;

        public double? WeaponHitChance => WeaponHits / WeaponHitAttempts.NullIfZero();
        public double? CritChance => Crits / Regulars.NullIfZero();
        public double? GlanceChance => Glances / Regulars.NullIfZero();
        public double? MissChance => Misses / WeaponHitAttempts.NullIfZero();

        public double? AverageWeaponDamage => WeaponDamage / WeaponHits.NullIfZero();
        public double? AverageRegularDamage => RegularDamage / Regulars.NullIfZero();
        public double? AverageNormalDamage => NormalDamage / Normals.NullIfZero();
        public double? AverageCritDamage => CritDamage / Crits.NullIfZero();
        public double? AverageGlanceDamage => GlanceDamage / Glances.NullIfZero();
        public double? AverageSpecialDamage => SpecialDamage / Specials.NullIfZero();
        public double? AverageNanoDamage => NanoDamage / NanoHits.NullIfZero();
        public double? AverageIndirectDamage => IndirectDamage / IndirectHits.NullIfZero();
        public double? AverageAbsorbedDamage => AbsorbedDamage / AbsorbedHits.NullIfZero();

        protected readonly Dictionary<DamageType, int> _damageTypeHits = new Dictionary<DamageType, int>();
        protected readonly Dictionary<DamageType, long> _damageTypeDamages = new Dictionary<DamageType, long>();
        public IReadOnlyDictionary<DamageType, int> DamageTypeHits => _damageTypeHits;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamages => _damageTypeDamages;

        public bool HasDamageTypeDamage(DamageType damageType) => DamageTypeDamages.ContainsKey(damageType);
        public bool HasSpecials => DamageTypeDamages.Keys.Any(DamageTypeHelpers.IsSpecialDamageType);
        public int? GetDamageTypeHits(DamageType damageType) => DamageTypeHits.TryGetValue(damageType, out int damageTypeHits) ? damageTypeHits : (int?)null;
        public long? GetDamageTypeDamage(DamageType damageType) => DamageTypeDamages.TryGetValue(damageType, out long damageTypeDamage) ? damageTypeDamage : (long?)null;
        public double? GetAverageDamageTypeDamage(DamageType damageType) => GetDamageTypeDamage(damageType) / (double?)GetDamageTypeHits(damageType);
        public double? GetSecondsPerDamageTypeHit(DamageType damageType) => Fight.Duration?.TotalSeconds / GetDamageTypeHits(damageType);
        public double? GetPercentDamageTypeDamage(DamageType damageType) => GetDamageTypeDamage(damageType) / (double?)TotalDamage;
        public double? GetPercentDamageTypeHits(DamageType damageType) => GetDamageTypeHits(damageType) / (double?)TotalHits;
    }
}
