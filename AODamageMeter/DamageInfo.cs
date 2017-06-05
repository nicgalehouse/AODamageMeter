using AODamageMeter.FightEvents;
using AODamageMeter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter
{
    // Just want to mention that we don't have DamageDoneInfo and DamageTakenInfo. Just DamageInfo, with the source and target specifying
    // who's doing the damage and who's taking it. And PlusPets in this context means plus the source's pets. We care about how much a
    // source and all his pets do against a target, we don't care about how much a target and all his pets take from a source. This is
    // reflected in FightCharacter by us not having any PlusPets for the Takens. My pets and I DPS together against a target, but we don't
    // tank together against a source--our HP bars are independent, so PlusPets just doesn't make as much sense there.
    public class DamageInfo
    {
        public DamageInfo(FightCharacter source, FightCharacter target)
        {
            Source = source;
            Target = target;
        }

        public FightCharacter Source { get; }
        public FightCharacter Target { get; }

        public long WeaponDamage { get; protected set; }
        public long CritDamage { get; protected set; }
        public long GlanceDamage { get; protected set; }
        public long NanoDamage { get; protected set; }
        public long IndirectDamage { get; protected set; }
        public long TotalDamage => WeaponDamage + NanoDamage + IndirectDamage;

        public long WeaponDamagePlusPets => WeaponDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.WeaponDamage ?? 0);
        public long CritDamagePlusPets => CritDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.CritDamage ?? 0);
        public long GlanceDamagePlusPets => GlanceDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.GlanceDamage ?? 0);
        public long NanoDamagePlusPets => NanoDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.NanoDamage ?? 0);
        public long IndirectDamagePlusPets => IndirectDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.IndirectDamage ?? 0);
        public long TotalDamagePlusPets => TotalDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.TotalDamage ?? 0);
        // FightCharacter creation & event handling guarantee if source has a fight pet owner, that owner has a damage info for Target.
        public long OwnersOrOwnTotalDamagePlusPets => Source.FightPetOwner?.DamageDoneInfosByTarget[Target].TotalDamagePlusPets ?? TotalDamagePlusPets;

        public double? WeaponPercentOfTotalDamage => WeaponDamage / TotalDamage.NullIfZero();
        public double? NanoPercentOfTotalDamage => NanoDamage / TotalDamage.NullIfZero();
        public double? IndirectPercentOfTotalDamage => IndirectDamage / TotalDamage.NullIfZero();

        public double? WeaponPercentOfTotalDamagePlusPets => WeaponDamagePlusPets / TotalDamagePlusPets.NullIfZero();
        public double? NanoPercentOfTotalDamagePlusPets => NanoDamagePlusPets / TotalDamagePlusPets.NullIfZero();
        public double? IndirectPercentOfTotalDamagePlusPets => IndirectDamagePlusPets / TotalDamagePlusPets.NullIfZero();

        public int WeaponHits { get; protected set; }
        public int Crits { get; protected set; }
        public int Glances { get; protected set; }
        public int Misses { get; protected set; }
        public int WeaponHitAttempts => WeaponHits + Misses;
        public int NanoHits { get; protected set; }
        public int IndirectHits { get; protected set; }
        public int TotalHits => WeaponHits + NanoHits + IndirectHits;

        public int WeaponHitsPlusPets => WeaponHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.WeaponHits ?? 0);
        public int CritsPlusPets => Crits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.Crits ?? 0);
        public int GlancesPlusPets => Glances + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.Glances ?? 0);
        public int MissesPlusPets => Misses + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.Misses ?? 0);
        public int WeaponHitAttemptsPlusPets => WeaponHitAttempts + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.WeaponHitAttempts ?? 0);
        public int NanoHitsPlusPets => NanoHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.NanoHits ?? 0);
        public int IndirectHitsPlusPets => IndirectHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.IndirectHits ?? 0);
        public int TotalHitsPlusPets => TotalHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.TotalHits ?? 0);

        public double? WeaponHitChance => WeaponHits / WeaponHitAttempts.NullIfZero();
        public double? CritChance => Crits / WeaponHitAttempts.NullIfZero();
        public double? GlanceChance => Glances / WeaponHitAttempts.NullIfZero();
        public double? MissChance => Misses / WeaponHitAttempts.NullIfZero();

        public double? WeaponHitChancePlusPets => WeaponHitsPlusPets / WeaponHitAttemptsPlusPets.NullIfZero();
        public double? CritChancePlusPets => CritsPlusPets / WeaponHitAttemptsPlusPets.NullIfZero();
        public double? GlanceChancePlusPets => GlancesPlusPets / WeaponHitAttemptsPlusPets.NullIfZero();
        public double? MissChancePlusPets => MissesPlusPets / WeaponHitAttemptsPlusPets.NullIfZero();

        public double? AverageWeaponDamage => WeaponDamage / WeaponHits.NullIfZero();
        public double? AverageCritDamage => CritDamage / Crits.NullIfZero();
        public double? AverageGlanceDamage => GlanceDamage / Glances.NullIfZero();
        public double? AverageNanoDamage => NanoDamage / NanoHits.NullIfZero();
        public double? AverageIndirectDamage => IndirectDamage / IndirectHits.NullIfZero();

        public double? AverageWeaponDamagePlusPets => WeaponDamagePlusPets / WeaponHitsPlusPets.NullIfZero();
        public double? AverageCritDamagePlusPets => CritDamagePlusPets / CritsPlusPets.NullIfZero();
        public double? AverageGlanceDamagePlusPets => GlanceDamagePlusPets / GlancesPlusPets.NullIfZero();
        public double? AverageNanoDamagePlusPets => NanoDamagePlusPets / NanoHitsPlusPets.NullIfZero();
        public double? AverageIndirectDamagePlusPets => IndirectDamagePlusPets / IndirectHitsPlusPets.NullIfZero();

        public double? PercentOfOwnersOrOwnTotalDamagePlusPets => TotalDamage / OwnersOrOwnTotalDamagePlusPets.NullIfZero();

        public double? PercentOfOwnersOrOwnTotalDamageDonePlusPets
            => Source.IsFightPet
            ? TotalDamage / Source.FightPetOwner.TotalDamageDonePlusPets.NullIfZero()
            : TotalDamage / Source.TotalDamageDonePlusPets.NullIfZero();
        public double? PercentOfOwnersOrOwnMaxDamageDonePlusPets
            => Source.IsFightPet
            ? TotalDamage / Source.FightPetOwner.MaxDamageDonePlusPets.NullIfZero()
            : TotalDamage / Source.MaxDamageDonePlusPets.NullIfZero();

        public double? PercentOfSourcesTotalDamageDone => TotalDamage / Source.TotalDamageDone.NullIfZero();
        public double? PercentOfSourcesMaxDamageDone => TotalDamage / Source.MaxDamageDone.NullIfZero();
        public double? PercentOfSourcesTotalDamageDonePlusPets => TotalDamage / Source.TotalDamageDonePlusPets.NullIfZero();
        public double? PercentOfSourcesMaxDamageDonePlusPets => TotalDamage / Source.MaxDamageDonePlusPets.NullIfZero();

        public double? PercentPlusPetsOfSourcesTotalDamageDonePlusPets => TotalDamagePlusPets / Source.TotalDamageDonePlusPets.NullIfZero();
        public double? PercentPlusPetsOfSourcesMaxDamageDonePlusPets => TotalDamagePlusPets / Source.MaxDamageDonePlusPets.NullIfZero();

        public double? PercentOfTargetsTotalDamageTaken => TotalDamage / Target.TotalDamageTaken.NullIfZero();
        public double? PercentOfTargetsMaxDamageTaken => TotalDamage / Target.MaxDamageTaken.NullIfZero();
        public double? PercentOfTargetsMaxDamagePlusPetsTaken => TotalDamage / Target.MaxDamagePlusPetsTaken.NullIfZero();

        public double? PercentPlusPetsOfTargetsTotalDamageTaken => TotalDamagePlusPets / Target.TotalDamageTaken.NullIfZero();
        public double? PercentPlusPetsOfTargetsMaxDamagePlusPetsTaken => TotalDamagePlusPets / Target.MaxDamagePlusPetsTaken.NullIfZero();

        protected readonly Dictionary<DamageType, int> _damageTypeHits = new Dictionary<DamageType, int>();
        protected readonly Dictionary<DamageType, long> _damageTypeDamages = new Dictionary<DamageType, long>();
        public IReadOnlyDictionary<DamageType, int> DamageTypeHits => _damageTypeHits;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamages => _damageTypeDamages;

        public bool HasDamageTypeDamage(DamageType damageType) => DamageTypeDamages.ContainsKey(damageType);
        public bool HasSpecials => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamage);
        public int? GetDamageTypeHits(DamageType damageType) => DamageTypeHits.TryGetValue(damageType, out int damageTypeHits) ? damageTypeHits : (int?)null;
        public long? GetDamageTypeDamage(DamageType damageType) => DamageTypeDamages.TryGetValue(damageType, out long damageTypeDamage) ? damageTypeDamage : (long?)null;
        public double? GetAverageDamageTypeDamage(DamageType damageType) => GetDamageTypeDamage(damageType) / (double?)GetDamageTypeHits(damageType);

        public void AddAttackEvent(AttackEvent attackEvent)
        {
            if (attackEvent.Source != Source || attackEvent.Target != Target)
                throw new ArgumentException($"This damage info is for {Source} and {Target},"
                    + $" but you're adding an event for {attackEvent.Source} and {attackEvent.Target}.");

            switch (attackEvent.AttackResult)
            {
                case AttackResult.WeaponHit:
                    WeaponDamage += attackEvent.Amount.Value;
                    ++WeaponHits;
                    if (attackEvent.AttackModifier == AttackModifier.Crit)
                    {
                        CritDamage += attackEvent.Amount.Value;
                        ++Crits;
                    }
                    else if (attackEvent.AttackModifier == AttackModifier.Glance)
                    {
                        GlanceDamage += attackEvent.Amount.Value;
                        ++Glances;
                    }
                    break;
                case AttackResult.Missed:
                    ++Misses;
                    break;
                case AttackResult.NanoHit:
                    NanoDamage += attackEvent.Amount.Value;
                    ++NanoHits;
                    break;
                case AttackResult.IndirectHit:
                    IndirectDamage += attackEvent.Amount.Value;
                    ++IndirectHits;
                    break;
                default: throw new NotImplementedException();
            }

            if (attackEvent.DamageType.HasValue)
            {
                _damageTypeHits.Increment(attackEvent.DamageType.Value, 1);
                _damageTypeDamages.Increment(attackEvent.DamageType.Value, attackEvent.Amount ?? 0);
            }
        }
    }
}
