using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneInfoDetailRow : DetailRowBase
    {
        public DamageDoneInfoDetailRow(FightCharacter fightCharacter, FightCharacter target)
            : base(fightCharacter)
        {
            Source = FightCharacter;
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
                    string specialsDoneInfo = (DamageDoneInfo?.HasSpecials ?? false) ?
$@"

{DamageDoneInfo.GetSpecialsDoneInfo()}" : null;

                    return
$@"{DisplayIndex}. {Source.UncoloredName} -> {Target.UncoloredName}

{DamageDoneInfo?.WeaponHitChance.FormatPercent() ?? EmDash} weapon hit chance
  {DamageDoneInfo?.CritChance.FormatPercent() ?? EmDash} crit chance
  {DamageDoneInfo?.GlanceChance.FormatPercent() ?? EmDash} glance chance

{DamageDoneInfo.AverageWeaponDamage.Format()} weapon dmg / hit
  {DamageDoneInfo.AverageCritDamage.Format()} crit dmg / hit
  {DamageDoneInfo.AverageGlanceDamage.Format()} glance dmg / hit
{DamageDoneInfo.AverageNanoDamage.Format()} nano dmg / hit
{DamageDoneInfo.AverageIndirectDamage.Format()} indirect dmg / hit{specialsDoneInfo}";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            DamageDoneInfo = DamageDoneInfo ?? Source.DamageDoneInfosByTarget.GetValueOrFallback(Target);

            PercentWidth = DamageDoneInfo?.PercentOfOwnersOrOwnMaxDamageDonePlusPets ?? 0;
            double? percentDone = Settings.Default.ShowPercentOfTotalDamageDone
                ? DamageDoneInfo?.PercentOfOwnersOrOwnTotalDamageDonePlusPets : DamageDoneInfo?.PercentOfOwnersOrOwnMaxDamageDonePlusPets;
            RightText = $"{DamageDoneInfo?.TotalDamage.Format() ?? EmDash} ({DamageDoneInfo?.PercentOfOwnersOrOwnTotalDamagePlusPets.FormatPercent() ?? EmDash}, {percentDone.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
