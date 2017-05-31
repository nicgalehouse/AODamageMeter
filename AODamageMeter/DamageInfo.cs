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

        public double WeaponPercentOfTotalDamage => TotalDamage == 0 ? 0 : WeaponDamage / (double)TotalDamage;
        public double NanoPercentOfTotalDamage => TotalDamage == 0 ? 0 : NanoDamage / (double)TotalDamage;
        public double IndirectPercentOfTotalDamage => TotalDamage == 0 ? 0 : IndirectDamage / (double)TotalDamage;

        public double WeaponPercentOfTotalDamagePlusPets => TotalDamagePlusPets == 0 ? 0 : WeaponDamagePlusPets / (double)TotalDamagePlusPets;
        public double NanoPercentOfTotalDamagePlusPets => TotalDamagePlusPets == 0 ? 0 : NanoDamagePlusPets / (double)TotalDamagePlusPets;
        public double IndirectPercentOfTotalDamagePlusPets => TotalDamagePlusPets == 0 ? 0 : IndirectDamagePlusPets / (double)TotalDamagePlusPets;

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

        public double WeaponHitChance => WeaponHitAttempts == 0 ? 0 : WeaponHits / (double)WeaponHitAttempts;
        public double CritChance => WeaponHitAttempts == 0 ? 0 : Crits / (double)WeaponHitAttempts;
        public double GlanceChance => WeaponHitAttempts == 0 ? 0 : Glances / (double)WeaponHitAttempts;
        public double MissChance => WeaponHitAttempts == 0 ? 0 : Misses / (double)WeaponHitAttempts;

        public double WeaponHitChancePlusPets => WeaponHitAttemptsPlusPets == 0 ? 0 : WeaponHitsPlusPets / (double)WeaponHitAttemptsPlusPets;
        public double CritChancePlusPets => WeaponHitAttemptsPlusPets == 0 ? 0 : CritsPlusPets / (double)WeaponHitAttemptsPlusPets;
        public double GlanceChancePlusPets => WeaponHitAttemptsPlusPets == 0 ? 0 : GlancesPlusPets / (double)WeaponHitAttemptsPlusPets;
        public double MissChancePlusPets => WeaponHitAttemptsPlusPets == 0 ? 0 : MissesPlusPets / (double)WeaponHitAttemptsPlusPets;

        public double AverageWeaponDamage => WeaponHits == 0 ? 0 : WeaponDamage / (double)WeaponHits;
        public double AverageCritDamage => Crits == 0 ? 0 : CritDamage / (double)Crits;
        public double AverageGlanceDamage => Glances == 0 ? 0 : GlanceDamage / (double)Glances;
        public double AverageNanoDamage => NanoHits == 0 ? 0 : NanoDamage / (double)NanoHits;
        public double AverageIndirectDamage => IndirectHits == 0 ? 0 : IndirectDamage / (double)IndirectHits;

        public double AverageWeaponDamagePlusPets => WeaponHitsPlusPets == 0 ? 0 : WeaponDamagePlusPets / (double)WeaponHitsPlusPets;
        public double AverageCritDamagePlusPets => CritsPlusPets == 0 ? 0 : CritDamagePlusPets / (double)CritsPlusPets;
        public double AverageGlanceDamagePlusPets => GlancesPlusPets == 0 ? 0 : GlanceDamagePlusPets / (double)GlancesPlusPets;
        public double AverageNanoDamagePlusPets => NanoHitsPlusPets == 0 ? 0 : NanoDamagePlusPets / (double)NanoHitsPlusPets;
        public double AverageIndirectDamagePlusPets => IndirectHitsPlusPets == 0 ? 0 : IndirectDamagePlusPets / (double)IndirectHitsPlusPets;

        public double PercentOfOwnersOrOwnTotalDamagePlusPets => OwnersOrOwnTotalDamagePlusPets == 0 ? 0 : TotalDamage / (double)OwnersOrOwnTotalDamagePlusPets;

        public double PercentOfOwnersOrOwnTotalDamageDonePlusPets
            => Source.IsFightPet
            ? Source.FightPetOwner.TotalDamageDonePlusPets == 0 ? 0 : TotalDamage / (double)Source.FightPetOwner.TotalDamageDonePlusPets
            : Source.TotalDamageDonePlusPets == 0 ? 0 : TotalDamage / (double)Source.TotalDamageDonePlusPets;
        public double PercentOfOwnersOrOwnMaxDamageDonePlusPets
            => Source.IsFightPet
            ? Source.FightPetOwner.MaxDamageDonePlusPets == 0 ? 0 : TotalDamage / (double)Source.FightPetOwner.MaxDamageDonePlusPets
            : Source.MaxDamageDonePlusPets == 0 ? 0 : TotalDamage / (double)Source.MaxDamageDonePlusPets;

        public double PercentOfSourcesTotalDamageDone => Source.TotalDamageDone == 0 ? 0 : TotalDamage / (double)Source.TotalDamageDone;
        public double PercentOfSourcesMaxDamageDone => Source.MaxDamageDone == 0 ? 0 : TotalDamage / (double)Source.MaxDamageDone;
        public double PercentOfSourcesTotalDamageDonePlusPets => Source.TotalDamageDonePlusPets == 0 ? 0 : TotalDamage / (double)Source.TotalDamageDonePlusPets;
        public double PercentOfSourcesMaxDamageDonePlusPets => Source.MaxDamageDonePlusPets == 0 ? 0 : TotalDamage / (double)Source.MaxDamageDonePlusPets;

        public double PercentPlusPetsOfSourcesTotalDamageDonePlusPets => Source.TotalDamageDonePlusPets == 0 ? 0 : TotalDamagePlusPets / (double)Source.TotalDamageDonePlusPets;
        public double PercentPlusPetsOfSourcesMaxDamageDonePlusPets => Source.MaxDamageDonePlusPets == 0 ? 0 : TotalDamagePlusPets / (double)Source.MaxDamageDonePlusPets;

        public double PercentOfTargetsTotalDamageTaken => Target.TotalDamageTaken == 0 ? 0 : TotalDamage / (double)Target.TotalDamageTaken;
        public double PercentOfTargetsMaxDamageTaken => Target.MaxDamageTaken == 0 ? 0 : TotalDamage / (double)Target.MaxDamageTaken;
        public double PercentOfTargetsMaxDamagePlusPetsTaken => Target.MaxDamagePlusPetsTaken == 0 ? 0 : TotalDamage / (double)Target.MaxDamagePlusPetsTaken;

        public double PercentPlusPetsOfTargetsTotalDamageTaken => Target.TotalDamageTaken == 0 ? 0 : TotalDamagePlusPets / (double)Target.TotalDamageTaken;
        public double PercentPlusPetsOfTargetsMaxDamagePlusPetsTaken => Target.MaxDamagePlusPetsTaken == 0 ? 0 : TotalDamagePlusPets / (double)Target.MaxDamagePlusPetsTaken;

        protected readonly Dictionary<DamageType, int> _damageTypeHits = new Dictionary<DamageType, int>();
        protected readonly Dictionary<DamageType, long> _damageTypeDamages = new Dictionary<DamageType, long>();
        public IReadOnlyDictionary<DamageType, int> DamageTypeHits => _damageTypeHits;
        public IReadOnlyDictionary<DamageType, long> DamageTypeDamages => _damageTypeDamages;

        public bool HasDamageTypeDamage(DamageType damageType)
            => DamageTypeDamages.ContainsKey(damageType);

        public bool HasSpecials
            => DamageTypeHelpers.SpecialDamageTypes.Any(HasDamageTypeDamage);

        public int? GetAverageDamageTypeDamage(DamageType damageType)
            => DamageTypeDamages.TryGetValue(damageType, out long damageTypeDamage)
            ? (int?)(damageTypeDamage / DamageTypeHits[damageType]) : null;

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
