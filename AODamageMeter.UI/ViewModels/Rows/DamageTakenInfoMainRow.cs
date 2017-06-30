using AODamageMeter.Helpers;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageTakenInfoMainRow : FightCharacterMainRowBase
    {
        public DamageTakenInfoMainRow(FightViewModel fightViewModel, DamageInfo damageTakenInfo)
            : base(fightViewModel, damageTakenInfo.Source)
             => DamageTakenInfo = damageTakenInfo;

        public DamageInfo DamageTakenInfo { get; }
        public FightCharacter Target => DamageTakenInfo.Target;
        public FightCharacter Source => DamageTakenInfo.Source;

        public override string Title => $"{Target.UncoloredName}'s Damage Taken from {Source.UncoloredName}";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{DisplayIndex}. {Title}

{DamageTakenInfo.TotalDamagePlusPets:N0} total dmg
{PercentOfTotal.FormatPercent()} of {Target.UncoloredName}'s total dmg
{PercentOfMax.FormatPercent()} of {Target.UncoloredName}'s max dmg

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
            PercentOfTotal = DamageTakenInfo.PercentPlusPetsOfTargetsTotalDamageTaken;
            PercentOfMax = DamageTakenInfo.PercentPlusPetsOfTargetsMaxDamagePlusPetsTaken;
            RightText = $"{DamageTakenInfo.TotalDamagePlusPets.Format()} ({DisplayedPercent.FormatPercent()})";

            if (Source.IsFightPetMaster)
            {
                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { Source }.Concat(Source.FightPets)
                    .OrderByDescending(c => c.DamageDoneInfosByTarget.GetValueOrFallback(Target)?.TotalDamage ?? 0)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new DamageTakenInfoDetailRow(FightViewModel, Target, fightCharacter));
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
