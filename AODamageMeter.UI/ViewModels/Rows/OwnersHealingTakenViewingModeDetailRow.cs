using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingTakenViewingModeDetailRow : FightCharacterDetailRowBase
    {
        public OwnersHealingTakenViewingModeDetailRow(FightViewModel fightViewModel, HealingInfo healingTakenInfo)
            : base(fightViewModel, healingTakenInfo.Source, showIcon: true)
            => HealingTakenInfo = healingTakenInfo;

        public HealingInfo HealingTakenInfo { get; }
        public FightCharacter Source => HealingTakenInfo.Source;
        public bool IsOwnerTheSource => Source.IsOwner;

        public override string Title => $"{Owner.UncoloredName}'s Healing Taken from {Source.UncoloredName}";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{DisplayIndex}. {Owner.UncoloredName} <- {Source.UncoloredName}

{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo.PotentialHealingPlusPets.Format()} potential healing
{HealingTakenInfo.RealizedHealingPlusPets.Format()} realized healing
{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo.OverhealingPlusPets.Format()} overhealing
{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo.NanoHealingPlusPets.Format()} nano healing

{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo.PercentOfOverhealingPlusPets.FormatPercent()} overhealing";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            if (!Source.IsFightPetOwner)
            {
                PercentWidth = HealingTakenInfo.PercentOfTargetsMaxPotentialHealingPlusPetsTaken ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? HealingTakenInfo.PercentOfTargetsPotentialHealingTaken : HealingTakenInfo.PercentOfTargetsMaxPotentialHealingPlusPetsTaken;
                RightText = $"{HealingTakenInfo.PotentialHealing.Format()} ({percentDone.FormatPercent()})";
            }
            else
            {
                PercentWidth = HealingTakenInfo.PercentPlusPetsOfTargetsMaxPotentialHealingPlusPetsTaken ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? HealingTakenInfo.PercentPlusPetsOfTargetsPotentialHealingTaken : HealingTakenInfo.PercentPlusPetsOfTargetsMaxPotentialHealingPlusPetsTaken;
                RightText = $"{HealingTakenInfo.PotentialHealingPlusPets.Format()} ({percentDone.FormatPercent()})";
            }

            base.Update(displayIndex);
        }
    }
}
