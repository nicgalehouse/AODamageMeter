using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingDoneMainRow : MainRowBase
    {
        public OwnersHealingDoneMainRow(HealingInfo healingDoneInfo)
            : base(healingDoneInfo.Target)
            => HealingDoneInfo = healingDoneInfo;

        public HealingInfo HealingDoneInfo { get; }
        public FightCharacter Source => HealingDoneInfo.Source;
        public FightCharacter Target => HealingDoneInfo.Target;
        public bool IsOwnerTheTarget => Target.IsDamageMeterOwner;

        public override string RightTextToolTip
        {
            get
            {
                lock (Source.DamageMeter)
                {
                    return
$@"{DisplayIndex}. {Source.UncoloredName} -> {Target.UncoloredName}

{(IsOwnerTheTarget ? "≥ " : "")}{HealingDoneInfo.PotentialHealingPlusPets.Format()} potential healing
{(IsOwnerTheTarget ? "" : "≥ ")}{HealingDoneInfo.RealizedHealingPlusPets.Format()} realized healing
≥ {HealingDoneInfo.OverhealingPlusPets.Format()} overhealing
{(IsOwnerTheTarget ? "≥ " : "")}{HealingDoneInfo.NanoHealingPlusPets.Format()} nano healing

≥ {HealingDoneInfo.PercentOfOverhealingPlusPets.FormatPercent()} overhealing";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            if (!Source.IsFightPetOwner)
            {
                PercentWidth = HealingDoneInfo.PercentOfSourcesMaxPotentialHealingDone ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? HealingDoneInfo.PercentOfSourcesPotentialHealingDone : HealingDoneInfo.PercentOfSourcesMaxPotentialHealingDone;
                RightText = $"{HealingDoneInfo.PotentialHealing.Format()} ({percentDone.FormatPercent()})";
            }
            else
            {
                PercentWidth = HealingDoneInfo.PercentPlusPetsOfSourcesMaxPotentialHealingDonePlusPets ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? HealingDoneInfo.PercentPlusPetsOfSourcesPotentialHealingDonePlusPets : HealingDoneInfo.PercentPlusPetsOfSourcesMaxPotentialHealingDonePlusPets;
                RightText = $"{HealingDoneInfo.PotentialHealingPlusPets.Format()} ({percentDone.FormatPercent()})";

                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { Source }.Concat(Source.FightPets)
                    .OrderByDescending(c => c.HealingDoneInfosByTarget.GetValueOrFallback(Target)?.PotentialHealing ?? 0)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new OwnersHealingDoneDetailRow(fightCharacter, Target));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            base.Update(displayIndex);
        }
    }
}
