using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageTakenInfoMainRow : FightCharacterMainRowBase
    {
        public DamageTakenInfoMainRow(DamageMeterViewModel damageMeterViewModel, DamageInfo damageTakenInfo)
            : base(damageMeterViewModel, damageTakenInfo.Source)
             => DamageTakenInfo = damageTakenInfo;

        public override string Title => $"{Target.UncoloredName}'s Damage Taken from {Source.UncoloredName}";

        public DamageInfo DamageTakenInfo { get; }
        public FightCharacter Target => DamageTakenInfo.Target;
        public FightCharacter Source => DamageTakenInfo.Source;

        public override string RightTextToolTip
        {
            get
            {
                lock (CurrentDamageMeter)
                {
                    return
$@"{DisplayIndex}. {Target.UncoloredName} <- {Source.UncoloredName}

{DamageTakenInfo.TotalDamage.ToString("N0")} total dmg

{(DamageTakenInfo.HasIncompleteMissStatsPlusPets ? "≤ " : "")}{DamageTakenInfo.WeaponHitChancePlusPets.FormatPercent()} weapon hit chance
  {DamageTakenInfo.CritChancePlusPets.FormatPercent()} crit chance
  {DamageTakenInfo.GlanceChancePlusPets.FormatPercent()} glance chance

{DamageTakenInfo.AverageWeaponDamagePlusPets.Format()} weapon dmg / hit
  {DamageTakenInfo.AverageCritDamagePlusPets.Format()} crit dmg / hit
  {DamageTakenInfo.AverageGlanceDamagePlusPets.Format()} glance dmg / hit
{DamageTakenInfo.AverageNanoDamagePlusPets.Format()} nano dmg / hit
{DamageTakenInfo.AverageIndirectDamagePlusPets.Format()} indirect dmg / hit"
+ (!DamageTakenInfo.HasSpecials ? null : $@"

{DamageTakenInfo.GetSpecialsInfo()}");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            if (!Source.IsFightPetOwner)
            {
                PercentWidth = DamageTakenInfo.PercentOfTargetsMaxDamagePlusPetsTaken ?? 0;
                double? percentTaken = Settings.Default.ShowPercentOfTotal
                    ? DamageTakenInfo.PercentOfTargetsTotalDamageTaken : DamageTakenInfo.PercentOfTargetsMaxDamagePlusPetsTaken;
                RightText = $"{DamageTakenInfo.TotalDamage.Format()} ({percentTaken.FormatPercent()})";
            }
            else
            {
                PercentWidth = DamageTakenInfo.PercentPlusPetsOfTargetsMaxDamagePlusPetsTaken ?? 0;
                double? percentTaken = Settings.Default.ShowPercentOfTotal
                    ? DamageTakenInfo.PercentPlusPetsOfTargetsTotalDamageTaken : DamageTakenInfo.PercentPlusPetsOfTargetsMaxDamagePlusPetsTaken;
                RightText = $"{DamageTakenInfo.TotalDamagePlusPets.Format()} ({percentTaken.FormatPercent()})";

                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { Source }.Concat(Source.FightPets)
                    .OrderByDescending(c => c.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.TotalDamage ?? 0)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new DamageTakenInfoDetailRow(DamageMeterViewModel, Target, fightCharacter));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            base.Update(displayIndex);
        }
    }
}
