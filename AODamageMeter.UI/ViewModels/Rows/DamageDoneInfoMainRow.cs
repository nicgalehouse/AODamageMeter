using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageDoneInfoMainRow : MainRowBase
    {
        public DamageDoneInfoMainRow(DamageInfo damageDoneInfo)
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

{DamageDoneInfo.GetSpecialsInfo()}" : null;

                    return
$@"{DisplayIndex}. {Source.UncoloredName} -> {Target.UncoloredName}

{DamageDoneInfo.WeaponHitChancePlusPets.FormatPercent()} weapon hit chance
  {DamageDoneInfo.CritChancePlusPets.FormatPercent()} crit chance
  {DamageDoneInfo.GlanceChancePlusPets.FormatPercent()} glance chance

{DamageDoneInfo.AverageWeaponDamagePlusPets.Format()} weapon dmg / hit
  {DamageDoneInfo.AverageCritDamagePlusPets.Format()} crit dmg / hit
  {DamageDoneInfo.AverageGlanceDamagePlusPets.Format()} glance dmg / hit
{DamageDoneInfo.AverageNanoDamagePlusPets.Format()} nano dmg / hit
{DamageDoneInfo.AverageIndirectDamagePlusPets.Format()} indirect dmg / hit{specialsDoneInfo}";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            if (!Source.IsFightPetOwner)
            {
                PercentWidth = DamageDoneInfo.PercentOfSourcesMaxDamageDone ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? DamageDoneInfo.PercentOfSourcesTotalDamageDone : DamageDoneInfo.PercentOfSourcesMaxDamageDone;
                RightText = $"{DamageDoneInfo.TotalDamage.Format()} ({percentDone.FormatPercent()})";
            }
            else
            {
                PercentWidth = DamageDoneInfo.PercentPlusPetsOfSourcesMaxDamageDonePlusPets ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? DamageDoneInfo.PercentPlusPetsOfSourcesTotalDamageDonePlusPets : DamageDoneInfo.PercentPlusPetsOfSourcesMaxDamageDonePlusPets;
                RightText = $"{DamageDoneInfo.TotalDamagePlusPets.Format()} ({percentDone.FormatPercent()})";

                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { Source }.Concat(Source.FightPets)
                    .OrderByDescending(c => c.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.TotalDamage ?? 0)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new DamageDoneInfoDetailRow(fightCharacter, Target));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            base.Update(displayIndex);
        }
    }
}
