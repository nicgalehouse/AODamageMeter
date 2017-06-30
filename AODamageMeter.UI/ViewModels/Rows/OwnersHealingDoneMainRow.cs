using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingDoneMainRow : FightCharacterMainRowBase
    {
        public OwnersHealingDoneMainRow(FightViewModel fightViewModel, HealingInfo healingDoneInfo)
            : base(fightViewModel, healingDoneInfo.Target)
            => HealingDoneInfo = healingDoneInfo;

        public HealingInfo HealingDoneInfo { get; }
        public FightCharacter Target => HealingDoneInfo.Target;

        public override string Title => $"{Owner.UncoloredName}'s Healing Done to {Target.UncoloredName}";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return HealingDoneInfo.GetOwnersHealingDoneTooltip(Title, DisplayIndex, PercentOfTotal, PercentOfMax);
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentOfTotal = HealingDoneInfo.PercentPlusPetsOfSourcesPotentialHealingDonePlusPets;
            PercentOfMax = HealingDoneInfo.PercentPlusPetsOfSourcesMaxPotentialHealingDonePlusPets;
            RightText = $"{HealingDoneInfo.PotentialHealingPlusPets.Format()} ({DisplayedPercent.FormatPercent()})";

            if (FightOwner.IsFightPetMaster)
            {
                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { FightOwner }.Concat(FightOwner.FightPets)
                    .OrderByDescending(c => c.HealingDoneInfosByTarget.GetValueOrFallback(Target)?.PotentialHealing ?? 0)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new OwnersHealingDoneDetailRow(FightViewModel, fightCharacter, Target));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            CleanUpOldPetDetailRowsIfNecessary(FightOwner);

            base.Update(displayIndex);
        }
    }
}
