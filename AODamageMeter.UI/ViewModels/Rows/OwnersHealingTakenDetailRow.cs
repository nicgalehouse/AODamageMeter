using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

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
$@"{DisplayIndex}. {Owner.UncoloredName} <- {Source.UncoloredName}

{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo?.PotentialHealing.Format() ?? EmDash} potential healing
{HealingTakenInfo?.RealizedHealing.Format() ?? EmDash} realized healing
{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo?.Overhealing.Format() ?? EmDash} overhealing
{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo?.NanoHealing.Format() ?? EmDash} nano healing

{(IsOwnerTheSource ? "≥ " : "")}{HealingTakenInfo?.PercentOfOverhealing.FormatPercent() ?? EmDash} overhealing";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            HealingTakenInfo = HealingTakenInfo ?? FightOwner.HealingTakenInfosBySource.GetValueOrFallback(Source);

            PercentWidth = HealingTakenInfo?.PercentOfTargetsMaxPotentialHealingPlusPetsTaken ?? 0;
            double? percentDone = Settings.Default.ShowPercentOfTotal
                ? HealingTakenInfo?.PercentOfTargetsPotentialHealingTaken : HealingTakenInfo?.PercentOfTargetsMaxPotentialHealingPlusPetsTaken;
            RightText = $"{HealingTakenInfo?.PotentialHealing.Format() ?? EmDash} ({HealingTakenInfo?.PercentOfOwnersOrOwnPotentialHealingPlusPets.FormatPercent() ?? EmDash}, {percentDone.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
