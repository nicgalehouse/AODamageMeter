using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingDoneViewingModeDetailRow : FightCharacterDetailRowBase
    {
        public OwnersHealingDoneViewingModeDetailRow(FightViewModel fightViewModel, HealingInfo healingDoneInfo)
            : base(fightViewModel, healingDoneInfo.Target, showIcon: true)
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

            base.Update(displayIndex);
        }
    }
}
