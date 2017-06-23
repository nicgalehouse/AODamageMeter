using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingTakenViewingModeDetailRow : FightCharacterDetailRowBase
    {
        public OwnersHealingTakenViewingModeDetailRow(FightViewModel fightViewModel, HealingInfo healingTakenInfo)
            : base(fightViewModel, healingTakenInfo.Source, showIcon: true)
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

            base.Update(displayIndex);
        }
    }
}
