using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class OwnersHealingDoneDetailRow : FightCharacterDetailRowBase
    {
        public OwnersHealingDoneDetailRow(FightViewModel fightViewModel, FightCharacter source, FightCharacter target)
            : base(fightViewModel, source)
        {
            Source = source;
            Target = target;
        }

        public HealingInfo HealingDoneInfo { get; private set; }
        public FightCharacter Source { get; }
        public FightCharacter Target { get; }
        public bool IsOwnerTheTarget => Target.IsOwner;
        public bool IsOwnerTheTargetAndSource => Target.IsOwner && Source.IsOwner;
        public bool IsOwnerNotTheTargetOrTheSource => !Target.IsOwner && !Source.IsOwner;
        public bool IsOwnerTheTargetAndNotTheSource => Target.IsOwner && !Source.IsOwner;

        public override string Title => $"{Source.UncoloredName}'s Healing Done to {Target.UncoloredName} (Detail)";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{DisplayIndex}. {Title}

{(IsOwnerTheTargetAndSource || IsOwnerNotTheTargetOrTheSource ? "≥ " : "")}{HealingDoneInfo?.PotentialHealing.Format() ?? EmDash} ({PercentOfMastersOrOwnTotalPlusPets.FormatPercent()}) potential healing
{PercentOfTotal.FormatPercent()} of {Source.FightPetMasterOrSelf.UncoloredName}'s total healing
{PercentOfMax.FormatPercent()} of {Source.FightPetMasterOrSelf.UncoloredName}'s max healing

{(IsOwnerTheTarget ? "" : "≥ ")}{HealingDoneInfo?.RealizedHealing.Format() ?? EmDash} realized healing
{(IsOwnerTheTargetAndNotTheSource ? "" : "≥ ")}{HealingDoneInfo?.Overhealing.Format() ?? EmDash} overhealing
{(IsOwnerTheTargetAndSource || IsOwnerNotTheTargetOrTheSource ? "≥ " : "")}{HealingDoneInfo?.NanoHealing.Format() ?? EmDash} nano healing

{(IsOwnerTheTargetAndNotTheSource ? "" : "≥ ")}{HealingDoneInfo?.PercentOfOverhealing.FormatPercent() ?? EmDashPercent} overhealing";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            HealingDoneInfo = HealingDoneInfo ?? Source.HealingDoneInfosByTarget.GetValueOrFallback(Target);
            PercentOfTotal = HealingDoneInfo?.PercentOfMastersOrOwnPotentialHealingDoneDonePlusPets;
            PercentOfMax = HealingDoneInfo?.PercentOfMastersOrOwnMaxPotentialHealingDonePlusPets;
            PercentOfMastersOrOwnTotalPlusPets = HealingDoneInfo?.PercentOfMastersOrOwnPotentialHealingPlusPets;
            RightText = $"{HealingDoneInfo?.PotentialHealing.Format() ?? EmDash} ({PercentOfMastersOrOwnTotalPlusPets.FormatPercent() ?? EmDashPercent}, {DisplayedPercent.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
