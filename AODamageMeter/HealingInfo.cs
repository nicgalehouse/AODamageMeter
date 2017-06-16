using AODamageMeter.FightEvents;
using AODamageMeter.Helpers;
using System;
using System.Linq;

namespace AODamageMeter
{
    // Just want to mention that we don't have HealingDoneInfo and HealingTakenInfo. Just HealingInfo, with the source and target specifying
    // who's doing the healing and who's taking it. And PlusPets in this context means plus the source's pets. We care about how much a
    // source and all his pets heal a target, we don't care about how much a target and all his pets get healed from a source. This is
    // reflected in FightCharacter by us not having any PlusPets for the Takens. My pets and I heal a target together, but we don't get
    // healed together from a source--our HP bars are independent, so PlusPets just doesn't make as much sense there.
    public class HealingInfo
    {
        public HealingInfo(FightCharacter source, FightCharacter target)
        {
            Source = source;
            Target = target;
        }

        public FightCharacter Source { get; }
        public FightCharacter Target { get; }

        public long PotentialHealing { get; protected set; }
        public long RealizedHealing { get; protected set; }
        public long Overhealing { get; protected set; }
        public long NanoHealing { get; protected set; }

        public long PotentialHealingPlusPets => PotentialHealing + Source.FightPets.Sum(p => p.HealingDoneInfosByTarget.GetValueOrFallback(Target)?.PotentialHealing ?? 0);
        public long RealizedHealingPlusPets => RealizedHealing + Source.FightPets.Sum(p => p.HealingDoneInfosByTarget.GetValueOrFallback(Target)?.RealizedHealing ?? 0);
        public long OverhealingPlusPets => Overhealing + Source.FightPets.Sum(p => p.HealingDoneInfosByTarget.GetValueOrFallback(Target)?.Overhealing ?? 0);
        public long NanoHealingPlusPets => NanoHealing + Source.FightPets.Sum(p => p.HealingDoneInfosByTarget.GetValueOrFallback(Target)?.NanoHealing ?? 0);
        // FightCharacter creation & event handling guarantee if source has a fight pet owner, that owner has a healing info for Target.
        public long OwnersOrOwnPotentialHealingPlusPets => Source.FightPetOwner?.HealingDoneInfosByTarget[Target].PotentialHealingPlusPets ?? PotentialHealingPlusPets;

        public double? PercentOfOverhealing => Overhealing / PotentialHealing.NullIfZero();
        public double? PercentOfOverhealingPlusPets => OverhealingPlusPets / PotentialHealingPlusPets.NullIfZero();

        public double? PercentOfOwnersOrOwnPotentialHealingPlusPets => PotentialHealing / OwnersOrOwnPotentialHealingPlusPets.NullIfZero();

        public double? PercentOfOwnersOrOwnPotentialHealingDoneDonePlusPets
            => Source.IsFightPet
            ? PotentialHealing / Source.FightPetOwner.PotentialHealingDonePlusPets.NullIfZero()
            : PotentialHealing / Source.PotentialHealingDonePlusPets.NullIfZero();
        public double? PercentOfOwnersOrOwnMaxPotentialHealingDonePlusPets
            => Source.IsFightPet
            ? PotentialHealing / Source.FightPetOwner.MaxPotentialHealingDonePlusPets.NullIfZero()
            : PotentialHealing / Source.MaxPotentialHealingDonePlusPets.NullIfZero();

        public double? PercentOfSourcesPotentialHealingDone => PotentialHealing / Source.PotentialHealingDone.NullIfZero();
        public double? PercentOfSourcesMaxPotentialHealingDone => PotentialHealing / Source.MaxPotentialHealingDone.NullIfZero();
        public double? PercentOfSourcesPotentialHealingDonePlusPets => PotentialHealing / Source.PotentialHealingDonePlusPets.NullIfZero();
        public double? PercentOfSourcesMaxPotentialHealingDonePlusPets => PotentialHealing / Source.MaxPotentialHealingDonePlusPets.NullIfZero();

        public double? PercentPlusPetsOfSourcesPotentialHealingDonePlusPets => PotentialHealingPlusPets / Source.PotentialHealingDonePlusPets.NullIfZero();
        public double? PercentPlusPetsOfSourcesMaxPotentialHealingDonePlusPets => PotentialHealingPlusPets / Source.MaxPotentialHealingDonePlusPets.NullIfZero();

        public double? PercentOfTargetsPotentialHealingTaken => PotentialHealing / Target.PotentialHealingTaken.NullIfZero();
        public double? PercentOfTargetsMaxPotentialHealingTaken => PotentialHealing / Target.MaxPotentialHealingTaken.NullIfZero();
        public double? PercentOfTargetsMaxPotentialHealingPlusPetsTaken => PotentialHealing / Target.MaxPotentialHealingPlusPetsTaken.NullIfZero();

        public double? PercentPlusPetsOfTargetsPotentialHealingTaken => PotentialHealingPlusPets / Target.PotentialHealingTaken.NullIfZero();
        public double? PercentPlusPetsOfTargetsMaxPotentialHealingPlusPetsTaken => PotentialHealingPlusPets / Target.MaxPotentialHealingPlusPetsTaken.NullIfZero();

        public void AddHealEvent(HealEvent healEvent)
        {
            if (healEvent.Source != Source || healEvent.Target != Target)
                throw new ArgumentException($"This heal info is for {Source} and {Target},"
                    + $" but you're adding an event for {healEvent.Source} and {healEvent.Target}.");

            switch (healEvent.HealType)
            {
                case HealType.PotentialHealth:
                    PotentialHealing += healEvent.Amount.Value;
                    break;
                case HealType.RealizedHealth:
                    RealizedHealing += healEvent.Amount.Value;
                    if (healEvent.StartEvent != null)
                    {
                        Overhealing += healEvent.StartEvent.Amount.Value - healEvent.Amount.Value;
                    }
                    else // No corresponding potential, so adding realized is best we can do (happens when owner heals themselves).
                    {
                        PotentialHealing += healEvent.Amount.Value;
                    }
                    break;
                case HealType.Nano:
                    NanoHealing += healEvent.Amount.Value;
                    break;
                default: throw new NotImplementedException();
            }
        }
    }
}
