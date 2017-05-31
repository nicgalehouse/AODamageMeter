using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneInfoMainRowViewModel : MainRowViewModelBase
    {
        public DamageDoneInfoMainRowViewModel(DamageInfo damageDoneInfo)
            : base(damageDoneInfo.Target)
             => DamageDoneInfo = damageDoneInfo;

        public DamageInfo DamageDoneInfo { get; }
        public FightCharacter Source => DamageDoneInfo.Source;
        public FightCharacter Target => DamageDoneInfo.Target;

        public override string RightTextToolTip
        {
            get
            {
                lock (Source.DamageMeter)
                {
                    string specialsDoneInfo = DamageDoneInfo.HasSpecials ?
$@"

{DamageDoneInfo.GetSpecialsDoneInfo()}" : null;

                    return
$@"{DisplayIndex}. {Source.UncoloredName} -> {Target.UncoloredName}

{DamageDoneInfo.WeaponHitChancePlusPets.FormatPercent()} weapon hit chance
  {DamageDoneInfo.CritChancePlusPets.FormatPercent()} crit chance
  {DamageDoneInfo.GlanceChancePlusPets.FormatPercent()} glance chance

{(DamageDoneInfo.AverageWeaponDamagePlusPets == 0 ? "N/A" : DamageDoneInfo.AverageWeaponDamagePlusPets.Format())} weapon dmg / hit
  {(DamageDoneInfo.AverageCritDamagePlusPets == 0 ? "N/A" : DamageDoneInfo.AverageCritDamagePlusPets.Format())} crit dmg / hit
  {(DamageDoneInfo.AverageGlanceDamagePlusPets == 0 ? "N/A" : DamageDoneInfo.AverageGlanceDamagePlusPets.Format())} glance dmg / hit
{(DamageDoneInfo.AverageNanoDamagePlusPets == 0 ? "N/A" : DamageDoneInfo.AverageNanoDamagePlusPets.Format())} nano dmg / hit
{(DamageDoneInfo.AverageIndirectDamagePlusPets == 0 ? "N/A" : DamageDoneInfo.AverageIndirectDamagePlusPets.Format())} indirect dmg / hit{specialsDoneInfo}";
                }
            }
        }

        public override void Update(int displayIndex)
        {
            if (!Source.IsFightPetOwner)
            {
                PercentWidth = DamageDoneInfo.PercentOfSourcesMaxDamageDone;
                double percentDone = Settings.Default.ShowPercentOfTotalDamageDone ? DamageDoneInfo.PercentOfSourcesTotalDamageDone : PercentWidth;
                RightText = $"{DamageDoneInfo.TotalDamage.Format()} ({percentDone.FormatPercent()})";
            }
            else
            {
                PercentWidth = DamageDoneInfo.PercentPlusPetsOfSourcesMaxDamageDonePlusPets;
                double percentDone = Settings.Default.ShowPercentOfTotalDamageDone ? DamageDoneInfo.PercentPlusPetsOfSourcesTotalDamageDonePlusPets : PercentWidth;
                RightText = $"{DamageDoneInfo.TotalDamagePlusPets.Format()} ({percentDone.FormatPercent()})";

                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { Source }.Concat(Source.FightPets)
                    .OrderByDescending(c => c.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.TotalDamage ?? 0)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out RowViewModelBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new DamageDoneInfoDetailRowViewModel(fightCharacter, Target));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            base.Update(displayIndex);
        }
    }
}
