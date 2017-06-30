using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingTakenMainRow : FightCharacterMainRowBase
    {
        public OwnersHealingTakenMainRow(FightViewModel fightViewModel, HealingInfo healingTakenInfo)
            : base(fightViewModel, healingTakenInfo.Source)
            => HealingTakenInfo = healingTakenInfo;

        public HealingInfo HealingTakenInfo { get; }
        public FightCharacter Source => HealingTakenInfo.Source;

        public override string Title => $"{Owner.UncoloredName}'s Healing Taken from {Source.UncoloredName}";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return HealingTakenInfo.GetOwnersHealingTakenTooltip(Title, DisplayIndex, PercentOfTotal, PercentOfMax);
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentOfTotal = HealingTakenInfo.PercentPlusPetsOfTargetsPotentialHealingTaken;
            PercentOfMax = HealingTakenInfo.PercentPlusPetsOfTargetsMaxPotentialHealingPlusPetsTaken;
            RightText = $"{HealingTakenInfo.PotentialHealingPlusPets.Format()} ({DisplayedPercent.FormatPercent()})";

            if (Source.IsFightPetMaster)
            {
                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { Source }.Concat(Source.FightPets)
                    .OrderByDescending(c => c.HealingDoneInfosByTarget.GetValueOrFallback(FightOwner)?.PotentialHealing ?? 0)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new OwnersHealingTakenDetailRow(FightViewModel, fightCharacter));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            CleanUpOldPetDetailRowsIfNecessary(Source);

            base.Update(displayIndex);
        }
    }
}
