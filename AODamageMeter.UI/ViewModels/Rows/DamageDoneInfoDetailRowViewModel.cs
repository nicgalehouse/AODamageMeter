using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneInfoDetailRowViewModel : DetailRowViewModelBase
    {
        public DamageDoneInfoDetailRowViewModel(FightCharacter fightCharacter, FightCharacter target)
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

{DamageDoneInfo?.WeaponHitChance.FormatPercent() ?? "N/A"} weapon hit chance
  {DamageDoneInfo?.CritChance.FormatPercent() ?? "N/A"} crit chance
  {DamageDoneInfo?.GlanceChance.FormatPercent() ?? "N/A"} glance chance

{((DamageDoneInfo?.AverageWeaponDamage ?? 0) == 0 ? "N/A" : DamageDoneInfo.AverageWeaponDamage.Format())} weapon dmg / hit
  {((DamageDoneInfo?.AverageCritDamage ?? 0) == 0 ? "N/A" : DamageDoneInfo.AverageCritDamage.Format())} crit dmg / hit
  {((DamageDoneInfo?.AverageGlanceDamage ?? 0) == 0 ? "N/A" : DamageDoneInfo.AverageGlanceDamage.Format())} glance dmg / hit
{((DamageDoneInfo?.AverageNanoDamage ?? 0) == 0 ? "N/A" : DamageDoneInfo.AverageNanoDamage.Format())} nano dmg / hit
{((DamageDoneInfo?.AverageIndirectDamage ?? 0) == 0 ? "N/A" : DamageDoneInfo.AverageIndirectDamage.Format())} indirect dmg / hit{specialsDoneInfo}";
                }
            }
        }

        public override void Update(int displayIndex)
        {
            DamageDoneInfo = DamageDoneInfo ?? Source.DamageDoneInfosByTarget.GetValueOrFallback(Target);

            if (DamageDoneInfo == null)
            {
                PercentWidth = 0;
                RightText = $"0 (0.0%, 0.0%)";
            }
            else
            {
                PercentWidth = DamageDoneInfo.PercentOfOwnersOrOwnMaxDamageDonePlusPets;
                double percentDone = Settings.Default.ShowPercentOfTotalDamageDone ? DamageDoneInfo.PercentOfOwnersOrOwnTotalDamageDonePlusPets : PercentWidth;
                RightText = $"{DamageDoneInfo.TotalDamage.Format()} ({DamageDoneInfo.PercentOfOwnersOrOwnTotalDamagePlusPets.FormatPercent()}, {percentDone.FormatPercent()})";
            }

            base.Update(displayIndex);
        }
    }
}
