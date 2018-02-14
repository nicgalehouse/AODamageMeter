using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageDoneInfoDetailRow : FightCharacterDetailRowBase
    {
        public DamageDoneInfoDetailRow(FightViewModel fightViewModel, FightCharacter source, FightCharacter target)
            : base(fightViewModel, source)
        {
            Source = source;
            Target = target;
        }

        public DamageInfo DamageDoneInfo { get; private set; }
        public FightCharacter Source { get; }
        public FightCharacter Target { get; }

        public override string Title => $"{Source.UncoloredName}'s Damage Done to {Target.UncoloredName} (Detail)";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{DisplayIndex}. {Title}

{DamageDoneInfo?.TotalDamage.ToString("N0") ?? EmDash} ({PercentOfMastersOrOwnTotalPlusPets.FormatPercent()}) total dmg
{PercentOfTotal.FormatPercent()} of {Source.FightPetMasterOrSelf.UncoloredName}'s total dmg
{PercentOfMax.FormatPercent()} of {Source.FightPetMasterOrSelf.UncoloredName}'s max dmg

{DamageDoneInfo?.WeaponPercentOfTotalDamage.FormatPercent() ?? EmDashPercent} weapon dmg
{DamageDoneInfo?.NanoPercentOfTotalDamage.FormatPercent() ?? EmDashPercent} nano dmg
{DamageDoneInfo?.IndirectPercentOfTotalDamage.FormatPercent() ?? EmDashPercent} indirect dmg
{(!DamageDoneInfo?.HasCompleteAbsorbedDamageStats ?? false ? "≥ " : "")}{DamageDoneInfo?.AbsorbedPercentOfTotalDamage.FormatPercent() ?? EmDashPercent} absorbed dmg

{(!DamageDoneInfo?.HasCompleteMissStats ?? false ? "≤ " : "")}{DamageDoneInfo?.WeaponHitChance.FormatPercent() ?? EmDashPercent} weapon hit chance
  {DamageDoneInfo?.CritChance.FormatPercent() ?? EmDashPercent} crit chance
  {DamageDoneInfo?.GlanceChance.FormatPercent() ?? EmDashPercent} glance chance"
+ (!Fight.HasObservedBlockedHits ? null : $@"
  {(!DamageDoneInfo?.HasCompleteBlockedHitStats ?? false ? "≥ " : "")}{DamageDoneInfo?.BlockedHitChance.FormatPercent() ?? EmDashPercent} blocked hit chance")
+ $@"

{DamageDoneInfo?.AverageWeaponDamage.Format() ?? EmDash} weapon dmg / hit
  {DamageDoneInfo?.AverageRegularDamage.Format() ?? EmDash} regular dmg / hit
    {DamageDoneInfo?.AverageNormalDamage.Format() ?? EmDash} normal dmg / hit
    {DamageDoneInfo?.AverageCritDamage.Format() ?? EmDash} crit dmg / hit
    {DamageDoneInfo?.AverageGlanceDamage.Format() ?? EmDash} glance dmg / hit
  {DamageDoneInfo?.AverageSpecialDamage.Format() ?? EmDash} special dmg / hit
{DamageDoneInfo?.AverageNanoDamage.Format() ?? EmDash} nano dmg / hit
{DamageDoneInfo?.AverageIndirectDamage.Format() ?? EmDash} indirect dmg / hit
{DamageDoneInfo?.AverageAbsorbedDamage.Format() ?? EmDash} absorbed dmg / hit"
+ (!DamageDoneInfo?.HasSpecials ?? true ? null : $@"

{DamageDoneInfo.GetSpecialsInfo()}")
+ ((DamageDoneInfo?.TotalDamage ?? 0) == 0 ? null : $@"

{DamageDoneInfo.GetDamageTypesInfo()}");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            DamageDoneInfo = DamageDoneInfo ?? Source.DamageDoneInfosByTarget.GetValueOrFallback(Target);
            PercentOfTotal = DamageDoneInfo?.PercentOfMastersOrOwnTotalDamageDonePlusPets;
            PercentOfMax = DamageDoneInfo?.PercentOfMastersOrOwnMaxDamageDonePlusPets;
            PercentOfMastersOrOwnTotalPlusPets = DamageDoneInfo?.PercentOfMastersOrOwnTotalDamagePlusPets;
            RightText = $"{DamageDoneInfo?.TotalDamage.Format() ?? EmDash} ({PercentOfMastersOrOwnTotalPlusPets.FormatPercent() ?? EmDashPercent}, {DisplayedPercent.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
