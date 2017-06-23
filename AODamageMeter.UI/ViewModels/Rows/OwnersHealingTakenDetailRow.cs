using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingTakenDetailRow : FightCharacterDetailRowBase
    {
        public OwnersHealingTakenDetailRow(FightViewModel fightViewModel, FightCharacter source)
            : base(fightViewModel, source)
            => Source = source;

        public HealingInfo HealingTakenInfo { get; private set; }
        public FightCharacter Source { get; }
        public bool IsOwnerTheSource => Source.IsOwner;

        public override string Title => $"{Owner.UncoloredName}'s Healing Taken from {Source.UncoloredName} (Detail)";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{DisplayIndex}. {Title}

{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo?.PotentialHealing.Format() ?? EmDash} ({PercentOfOwnersOrOwnTotalPlusPets.FormatPercent()}) potential healing
{PercentOfTotal.FormatPercent()} of {Owner.UncoloredName}'s total healing
{PercentOfMax.FormatPercent()} of {Owner.UncoloredName}'s max healing

{HealingTakenInfo?.RealizedHealing.Format() ?? EmDash} realized healing
{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo?.Overhealing.Format() ?? EmDash} overhealing
{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo?.NanoHealing.Format() ?? EmDash} nano healing

{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo?.PercentOfOverhealing.FormatPercent() ?? EmDashPercent} overhealing";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            HealingTakenInfo = HealingTakenInfo ?? FightOwner.HealingTakenInfosBySource.GetValueOrFallback(Source);
            PercentOfTotal = HealingTakenInfo?.PercentOfTargetsPotentialHealingTaken;
            PercentOfMax = HealingTakenInfo?.PercentOfTargetsMaxPotentialHealingPlusPetsTaken;
            PercentOfOwnersOrOwnTotalPlusPets = HealingTakenInfo?.PercentOfOwnersOrOwnPotentialHealingPlusPets;
            RightText = $"{HealingTakenInfo?.PotentialHealing.Format() ?? EmDash} ({PercentOfOwnersOrOwnTotalPlusPets.FormatPercent() ?? EmDashPercent}, {DisplayedPercent.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
