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

        public DamageMeter DamageMeter => Source.DamageMeter;
        public FightCharacter Source { get; }
        public FightCharacter Target { get; }
        public IEnumerable<FightCharacter> SourceAndFightPets
        {
            get
            {
                yield return Source;

                foreach (var fightPet in Source.FightPets)
                    yield return fightPet;
            }
        }

        public long WeaponDamage { get; protected set; }
        public long RegularDamage { get; protected set; }
        public long NormalDamage { get; protected set; }
        public long CritDamage { get; protected set; }
        public long GlanceDamage { get; protected set; }
        public long SpecialDamage { get; protected set; }
        public long NanoDamage { get; protected set; }
        public long IndirectDamage { get; protected set; }
        public long AbsorbedDamage { get; protected set; }
        public long TotalDamage => WeaponDamage + NanoDamage + IndirectDamage + AbsorbedDamage;

        public long WeaponDamagePlusPets => WeaponDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.WeaponDamage ?? 0);
        public long RegularDamagePlusPets => RegularDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.RegularDamage ?? 0);
        public long NormalDamagePlusPets => NormalDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.NormalDamage ?? 0);
        public long CritDamagePlusPets => CritDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.CritDamage ?? 0);
        public long GlanceDamagePlusPets => GlanceDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.GlanceDamage ?? 0);
        public long SpecialDamagePlusPets => SpecialDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.SpecialDamage ?? 0);
        public long NanoDamagePlusPets => NanoDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.NanoDamage ?? 0);
        public long IndirectDamagePlusPets => IndirectDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.IndirectDamage ?? 0);
        public long AbsorbedDamagePlusPets => AbsorbedDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.AbsorbedDamage ?? 0);
        public long TotalDamagePlusPets => TotalDamage + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.TotalDamage ?? 0);
        // FightPet registration handling guarantees if source has a fight pet master, that master has a damage info for Target.
        public long MastersOrOwnTotalDamagePlusPets => Source.FightPetMaster?.DamageDoneInfosByTarget[Target].TotalDamagePlusPets ?? TotalDamagePlusPets;

        public double? WeaponPercentOfTotalDamage => WeaponDamage / TotalDamage.NullIfZero();
        public double? NanoPercentOfTotalDamage => NanoDamage / TotalDamage.NullIfZero();
        public double? IndirectPercentOfTotalDamage => IndirectDamage / TotalDamage.NullIfZero();
        public double? AbsorbedPercentOfTotalDamage => AbsorbedDamage / TotalDamage.NullIfZero();

        public double? WeaponPercentOfTotalDamagePlusPets => WeaponDamagePlusPets / TotalDamagePlusPets.NullIfZero();
        public double? NanoPercentOfTotalDamagePlusPets => NanoDamagePlusPets / TotalDamagePlusPets.NullIfZero();
        public double? IndirectPercentOfTotalDamagePlusPets => IndirectDamagePlusPets / TotalDamagePlusPets.NullIfZero();
        public double? AbsorbedPercentOfTotalDamagePlusPets => AbsorbedDamagePlusPets / TotalDamagePlusPets.NullIfZero();

        public int WeaponHits { get; protected set; }
        public int Regulars { get; protected set; }
        public int Normals { get; protected set; }
        public int Crits { get; protected set; }
        public int Glances { get; protected set; }
        public int BlockedHits { get; protected set; }
        public int NonBlockedRegulars => Regulars - BlockedHits;
        public int Specials { get; protected set; }
        public int Misses { get; protected set; }
        public int WeaponHitAttempts => WeaponHits + Misses;
        public int NanoHits { get; protected set; }
        public int IndirectHits { get; protected set; }
        public int AbsorbedHits { get; protected set; }
        public int TotalHits => WeaponHits + NanoHits + IndirectHits + AbsorbedHits;
        public int TotalNonBlockedHits => TotalHits - BlockedHits;

        public int WeaponHitsPlusPets => WeaponHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.WeaponHits ?? 0);
        public int RegularsPlusPets => Regulars + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.Regulars ?? 0);
        public int NormalsPlusPets => Normals + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.Normals ?? 0);
        public int CritsPlusPets => Crits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.Crits ?? 0);
        public int GlancesPlusPets => Glances + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.Glances ?? 0);
        public int BlockedHitsPlusPets => BlockedHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.BlockedHits ?? 0);
        public int NonBlockedRegularsPlusPets => NonBlockedRegulars + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.NonBlockedRegulars ?? 0);
        public int SpecialsPlusPets => Specials + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.Specials ?? 0);
        public int MissesPlusPets => Misses + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.Misses ?? 0);
        public int WeaponHitAttemptsPlusPets => WeaponHitAttempts + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.WeaponHitAttempts ?? 0);
        public int NanoHitsPlusPets => NanoHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.NanoHits ?? 0);
        public int IndirectHitsPlusPets => IndirectHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.IndirectHits ?? 0);
        public int AbsorbedHitsPlusPets => AbsorbedHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.AbsorbedHits ?? 0);
        public int TotalHitsPlusPets => TotalHits + Source.FightPets.Sum(p => p.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.TotalHits ?? 0);
        public int TotalNonBlockedHitsPlusPets => TotalHitsPlusPets - BlockedHitsPlusPets;

        public double? WeaponHitChance => WeaponHits / WeaponHitAttempts.NullIfZero();
        public double? CritChance => Crits / NonBlockedRegulars.NullIfZero();
        public double? GlanceChance => Glances / NonBlockedRegulars.NullIfZero();
        public double? BlockedHitChance => BlockedHits / Regulars.NullIfZero();
        public double? MissChance => Misses / WeaponHitAttempts.NullIfZero();

        public double? WeaponHitChancePlusPets => WeaponHitsPlusPets / WeaponHitAttemptsPlusPets.NullIfZero();
        public double? CritChancePlusPets => CritsPlusPets / NonBlockedRegularsPlusPets.NullIfZero();
        public double? GlanceChancePlusPets => GlancesPlusPets / NonBlockedRegularsPlusPets.NullIfZero();
        public double? BlockedHitChancePlusPets => BlockedHitsPlusPets / RegularsPlusPets.NullIfZero();
        public double? MissChancePlusPets => MissesPlusPets / WeaponHitAttemptsPlusPets.NullIfZero();

        public double? AverageWeaponDamage => WeaponDamage / WeaponHits.NullIfZero();
        public double? AverageRegularDamage => RegularDamage / Regulars.NullIfZero();
        public double? AverageNormalDamage => NormalDamage / Normals.NullIfZero();
        public double? AverageCritDamage => CritDamage / Crits.NullIfZero();
        public double? AverageGlanceDamage => GlanceDamage / Glances.NullIfZero();
        public double? AverageSpecialDamage => SpecialDamage / Specials.NullIfZero();
        public double? AverageNanoDamage => NanoDamage / NanoHits.NullIfZero();
        public double? AverageIndirectDamage => IndirectDamage / IndirectHits.NullIfZero();
        public double? AverageAbsorbedDamage => AbsorbedDamage / AbsorbedHits.NullIfZero();

        public double? AverageWeaponDamagePlusPets => WeaponDamagePlusPets / WeaponHitsPlusPets.NullIfZero();
        public double? AverageRegularDamagePlusPets => RegularDamagePlusPets / RegularsPlusPets.NullIfZero();
        public double? AverageNormalDamagePlusPets => NormalDamagePlusPets / NormalsPlusPets.NullIfZero();
        public double? AverageCritDamagePlusPets => CritDamagePlusPets / CritsPlusPets.NullIfZero();
        public double? AverageGlanceDamagePlusPets => GlanceDamagePlusPets / GlancesPlusPets.NullIfZero();
        public double? AverageSpecialDamagePlusPets => SpecialDamagePlusPets / SpecialsPlusPets.NullIfZero();
        public double? AverageNanoDamagePlusPets => NanoDamagePlusPets / NanoHitsPlusPets.NullIfZero();
        public double? AverageIndirectDamagePlusPets => IndirectDamagePlusPets / IndirectHitsPlusPets.NullIfZero();
        public double? AverageAbsorbedDamagePlusPets => AbsorbedDamagePlusPets / AbsorbedHitsPlusPets.NullIfZero();

        public double? PercentOfMastersOrOwnTotalDamagePlusPets => TotalDamage / MastersOrOwnTotalDamagePlusPets.NullIfZero();

        public double? PercentOfMastersOrOwnTotalDamageDonePlusPets
            => Source.IsFightPet
            ? TotalDamage / Source.FightPetMaster.TotalDamageDonePlusPets.NullIfZero()
            : TotalDamage / Source.TotalDamageDonePlusPets.NullIfZero();
        public double? PercentOfMastersOrOwnMaxDamageDonePlusPets
            => Source.IsFightPet
            ? TotalDamage / Source.FightPetMaster.MaxDamageDonePlusPets.NullIfZero()
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
        public bool HasSpecials => DamageTypeDamages.Keys.Any(DamageTypeHelpers.IsSpecialDamageType);
        public int? GetDamageTypeHits(DamageType damageType) => DamageTypeHits.TryGetValue(damageType, out int damageTypeHits) ? damageTypeHits : (int?)null;
        public long? GetDamageTypeDamage(DamageType damageType) => DamageTypeDamages.TryGetValue(damageType, out long damageTypeDamage) ? damageTypeDamage : (long?)null;
        public double? GetAverageDamageTypeDamage(DamageType damageType) => GetDamageTypeDamage(damageType) / (double?)GetDamageTypeHits(damageType);
        public double? GetPercentDamageTypeDamage(DamageType damageType) => GetDamageTypeDamage(damageType) / (double?)TotalDamage;
        public double? GetPercentDamageTypeHits(DamageType damageType) => GetDamageTypeHits(damageType) / (double?)TotalNonBlockedHits;

        public bool HasDamageTypeDamagePlusPets(DamageType damageType) => SourceAndFightPets.Any(c => c.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.HasDamageTypeDamage(damageType) ?? false);
        public bool HasSpecialsPlusPets => SourceAndFightPets.Any(c => c.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.HasSpecials ?? false);
        public int? GetDamageTypeHitsPlusPets(DamageType damageType) => SourceAndFightPets.NullableSum(c => c.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.GetDamageTypeHits(damageType));
        public long? GetDamageTypeDamagePlusPets(DamageType damageType) => SourceAndFightPets.NullableSum(c => c.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.GetDamageTypeDamage(damageType));
        public double? GetAverageDamageTypeDamagePlusPets(DamageType damageType) => GetDamageTypeDamagePlusPets(damageType) / (double?)GetDamageTypeHitsPlusPets(damageType);
        public double? GetPercentDamageTypeDamagePlusPets(DamageType damageType) => GetDamageTypeDamagePlusPets(damageType) / (double?)TotalDamagePlusPets;
        public double? GetPercentDamageTypeHitsPlusPets(DamageType damageType) => GetDamageTypeHitsPlusPets(damageType) / (double?)TotalNonBlockedHitsPlusPets;

        public bool HasCompleteBlockedHitStats => Source.IsOwner && Target.IsUnknown || Target.IsOwner && Source.IsUnknown;
        public bool HasCompleteBlockedHitStatsPlusPets => (Source.IsOwner && !Source.FightPets.Any() && Target.IsUnknown) || Target.IsOwner && Source.IsUnknown;

        public bool HasCompleteMissStats => Source.IsOwner || Target.IsOwner;
        public bool HasCompleteMissStatsPlusPets => (Source.IsOwner && !Source.FightPets.Any()) || Target.IsOwner;

        public bool HasCompleteAbsorbedDamageStats => (Target.IsOwner || Target.IsUnknown) && Source.IsUnknown;

        public void AddAttackEvent(AttackEvent attackEvent)
        {
            if (attackEvent.Source != Source || attackEvent.Target != Target)
                throw new ArgumentException($"This damage info is for {Source} and {Target},"
                    + $" but you're adding an event for {attackEvent.Source} and {attackEvent.Target}.");

            switch (attackEvent.AttackResult)
            {
                case AttackResult.WeaponHit:
                    WeaponDamage += attackEvent.Amount ?? 0;
                    ++WeaponHits;
                    // Logs don't report crits/glances for specials. Track normal hits so we can better approximate crit/glance chance.
                    if (!attackEvent.IsSpecialDamage)
                    {
                        RegularDamage += attackEvent.Amount ?? 0;
                        ++Regulars;
                        if (!attackEvent.AttackModifier.HasValue)
                        {
                            NormalDamage += attackEvent.Amount.Value;
                            ++Normals;
                        }
                        else if (attackEvent.AttackModifier == AttackModifier.Crit)
                        {
                            CritDamage += attackEvent.Amount.Value;
                            ++Crits;
                        }
                        else if (attackEvent.AttackModifier == AttackModifier.Glance)
                        {
                            GlanceDamage += attackEvent.Amount.Value;
                            ++Glances;
                        }
                        else if (attackEvent.AttackModifier == AttackModifier.Block)
                        {
                            ++BlockedHits;
                        }
                        else throw new NotImplementedException();
                    }
                    else
                    {
                        SpecialDamage += attackEvent.Amount.Value;
                        ++Specials;
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
                case AttackResult.Absorbed:
                    AbsorbedDamage += attackEvent.Amount.Value;
                    ++AbsorbedHits;
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
