using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageDoneInfoMainRow : FightCharacterMainRowBase
    {
        public DamageDoneInfoMainRow(FightViewModel fightViewModel, DamageInfo damageDoneInfo)
            : base(fightViewModel, damageDoneInfo.Target)
             => DamageDoneInfo = damageDoneInfo;

        public DamageInfo DamageDoneInfo { get; }
        public FightCharacter Source => DamageDoneInfo.Source;
        public FightCharacter Target => DamageDoneInfo.Target;

        public override string Title => $"{Source.UncoloredName}'s Damage Done to {Target.UncoloredName}";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{DisplayIndex}. {Title}

{DamageDoneInfo.TotalDamagePlusPets:N0} total dmg
{PercentOfTotal.FormatPercent()} of {Source.UncoloredName}'s total dmg
{PercentOfMax.FormatPercent()} of {Source.UncoloredName}'s max dmg

{DamageDoneInfo.WeaponPercentOfTotalDamagePlusPets.FormatPercent()} weapon dmg
{DamageDoneInfo.NanoPercentOfTotalDamagePlusPets.FormatPercent()} nano dmg
{DamageDoneInfo.IndirectPercentOfTotalDamagePlusPets.FormatPercent()} indirect dmg
{(!DamageDoneInfo.HasCompleteAbsorbedDamageStats ? "≥ " : "")}{DamageDoneInfo.AbsorbedPercentOfTotalDamagePlusPets.FormatPercent()} absorbed dmg

{(!DamageDoneInfo.HasCompleteMissStatsPlusPets ? "≤ " : "")}{DamageDoneInfo.WeaponHitChancePlusPets.FormatPercent()} weapon hit chance
  {DamageDoneInfo.CritChancePlusPets.FormatPercent()} crit chance
  {DamageDoneInfo.GlanceChancePlusPets.FormatPercent()} glance chance

{DamageDoneInfo.AverageWeaponDamagePlusPets.Format()} weapon dmg / hit
  {DamageDoneInfo.AverageCritDamagePlusPets.Format()} crit dmg / hit
  {DamageDoneInfo.AverageGlanceDamagePlusPets.Format()} glance dmg / hit
{DamageDoneInfo.AverageNanoDamagePlusPets.Format()} nano dmg / hit
{DamageDoneInfo.AverageIndirectDamagePlusPets.Format()} indirect dmg / hit
{DamageDoneInfo.AverageAbsorbedDamagePlusPets.Format()} absorbed dmg / hit"
+ (!DamageDoneInfo.HasSpecialsPlusPets ? null : $@"

{DamageDoneInfo.GetSpecialsPlusPetsInfo()}")
+ (DamageDoneInfo.TotalDamagePlusPets == 0 ? null : $@"

{DamageDoneInfo.GetDamageTypesPlusPetsInfo()}");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentOfTotal = DamageDoneInfo.PercentPlusPetsOfSourcesTotalDamageDonePlusPets;
            PercentOfMax = DamageDoneInfo.PercentPlusPetsOfSourcesMaxDamageDonePlusPets;
            RightText = $"{DamageDoneInfo.TotalDamagePlusPets.Format()} ({DisplayedPercent.FormatPercent()})";

            if (Source.IsFightPetMaster)
            {
                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { Source }.Concat(Source.FightPets)
                    .OrderByDescending(c => c.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.TotalDamage ?? 0)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new DamageDoneInfoDetailRow(FightViewModel, fightCharacter, Target));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            CleanUpOldPetDetailRowsIfNecessary(Source);

            base.Update(displayIndex);
        }
    }
}
