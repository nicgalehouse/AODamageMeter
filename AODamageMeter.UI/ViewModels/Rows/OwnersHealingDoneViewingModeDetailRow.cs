using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingDoneViewingModeDetailRow : FightCharacterDetailRowBase
    {
        public OwnersHealingDoneViewingModeDetailRow(FightViewModel fightViewModel, HealingInfo healingDoneInfo)
            : base(fightViewModel, healingDoneInfo.Target, showIcon: true)
            => HealingDoneInfo = healingDoneInfo;

        public HealingInfo HealingDoneInfo { get; }
        public FightCharacter Target => HealingDoneInfo.Target;
        public bool IsOwnerTheTarget => Target.IsOwner;

        public override string Title => $"{Owner.UncoloredName}'s Healing Done to {Target.UncoloredName}";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{DisplayIndex}. {Owner.UncoloredName} -> {Target.UncoloredName}

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
            if (!FightOwner.IsFightPetOwner)
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
            }

            base.Update(displayIndex);
        }
    }
}
