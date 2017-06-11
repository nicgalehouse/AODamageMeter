using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageDoneInfoDetailRow : DetailRowBase
    {
        public DamageDoneInfoDetailRow(FightCharacter source, FightCharacter target)
            : base(source)
        {
            Source = source;
            Target = target;
        }

        public DamageInfo DamageDoneInfo { get; private set; }
        public FightCharacter Source { get; }
        public FightCharacter Target { get; }

        public override string RightTextToolTip
        {
            get
            {
                lock (Source.DamageMeter)
                {
                    return
$@"{DisplayIndex}. {Source.UncoloredName} -> {Target.UncoloredName}

{DamageDoneInfo?.TotalDamage.ToString("N0") ?? EmDash} total dmg

{DamageDoneInfo?.WeaponHitChance.FormatPercent() ?? EmDash} weapon hit chance
  {DamageDoneInfo?.CritChance.FormatPercent() ?? EmDash} crit chance
  {DamageDoneInfo?.GlanceChance.FormatPercent() ?? EmDash} glance chance

{DamageDoneInfo?.AverageWeaponDamage.Format() ?? EmDash} weapon dmg / hit
  {DamageDoneInfo?.AverageCritDamage.Format() ?? EmDash} crit dmg / hit
  {DamageDoneInfo?.AverageGlanceDamage.Format() ?? EmDash} glance dmg / hit
{DamageDoneInfo?.AverageNanoDamage.Format() ?? EmDash} nano dmg / hit
{DamageDoneInfo?.AverageIndirectDamage.Format() ?? EmDash} indirect dmg / hit"
+ (!(DamageDoneInfo?.HasSpecials ?? false) ? null : $@"

{DamageDoneInfo.GetSpecialsInfo()}");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            DamageDoneInfo = DamageDoneInfo ?? Source.DamageDoneInfosByTarget.GetValueOrFallback(Target);

            PercentWidth = DamageDoneInfo?.PercentOfOwnersOrOwnMaxDamageDonePlusPets ?? 0;
            double? percentDone = Settings.Default.ShowPercentOfTotal
                ? DamageDoneInfo?.PercentOfOwnersOrOwnTotalDamageDonePlusPets : DamageDoneInfo?.PercentOfOwnersOrOwnMaxDamageDonePlusPets;
            RightText = $"{DamageDoneInfo?.TotalDamage.Format() ?? EmDash} ({DamageDoneInfo?.PercentOfOwnersOrOwnTotalDamagePlusPets.FormatPercent() ?? EmDash}, {percentDone.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
