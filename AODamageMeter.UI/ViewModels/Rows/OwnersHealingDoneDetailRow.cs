using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingDoneDetailRow : FightCharacterDetailRowBase
    {
        public OwnersHealingDoneDetailRow(DamageMeterViewModel damageMeterViewModel, FightCharacter source, FightCharacter target)
            : base(damageMeterViewModel, source)
        {
            Source = source;
            Target = target;
        }

        public override string Title => $"{Source.UncoloredName}'s Healing Done to {Target.UncoloredName} (Detail)";

        public HealingInfo HealingDoneInfo { get; private set; }
        public FightCharacter Source { get; }
        public FightCharacter Target { get; }
        public bool IsOwnerTheTarget => Target.IsDamageMeterOwner;
        public bool IsOwnerTheTargetAndSource => Target.IsDamageMeterOwner && Source.IsDamageMeterOwner;
        public bool IsOwnerNotTheTargetOrTheSource => !Target.IsDamageMeterOwner && !Source.IsDamageMeterOwner;
        public bool IsOwnerTheTargetAndNotTheSource => Target.IsDamageMeterOwner && !Source.IsDamageMeterOwner;

        public override string RightTextToolTip
        {
            get
            {
                lock (CurrentDamageMeter)
                {
                    return
$@"{DisplayIndex}. {Source.UncoloredName} -> {Target.UncoloredName}

{(IsOwnerTheTargetAndSource || IsOwnerNotTheTargetOrTheSource ? "≥ " : "")}{HealingDoneInfo?.PotentialHealing.Format() ?? EmDash} potential healing
{(IsOwnerTheTarget ? "" : "≥ ")}{HealingDoneInfo?.RealizedHealing.Format() ?? EmDash} realized healing
{(IsOwnerTheTargetAndNotTheSource ? "" : "≥ ")}{HealingDoneInfo?.Overhealing.Format() ?? EmDash} overhealing
{(IsOwnerTheTargetAndSource || IsOwnerNotTheTargetOrTheSource ? "≥ " : "")}{HealingDoneInfo?.NanoHealing.Format() ?? EmDash} nano healing

{(IsOwnerTheTargetAndNotTheSource ? "" : "≥ ")}{HealingDoneInfo?.PercentOfOverhealing.FormatPercent() ?? EmDash} overhealing";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            HealingDoneInfo = HealingDoneInfo ?? Source.HealingDoneInfosByTarget.GetValueOrFallback(Target);

            PercentWidth = HealingDoneInfo?.PercentOfOwnersOrOwnMaxPotentialHealingDonePlusPets ?? 0;
            double? percentDone = Settings.Default.ShowPercentOfTotal
                ? HealingDoneInfo?.PercentOfOwnersOrOwnPotentialHealingDoneDonePlusPets : HealingDoneInfo?.PercentOfOwnersOrOwnMaxPotentialHealingDonePlusPets;
            RightText = $"{HealingDoneInfo?.PotentialHealing.Format() ?? EmDash} ({HealingDoneInfo?.PercentOfOwnersOrOwnPotentialHealingPlusPets.FormatPercent() ?? EmDash}, {percentDone.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
