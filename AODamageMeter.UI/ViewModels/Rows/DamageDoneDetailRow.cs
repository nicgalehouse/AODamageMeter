using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageDoneDetailRow : FightCharacterDetailRowBase
    {
        public DamageDoneDetailRow(FightViewModel fightViewModel, FightCharacter fightCharacter)
            : base(fightViewModel, fightCharacter)
        { }

        public override string Title => $"{FightCharacterName}'s Damage Done (Detail)";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{DisplayIndex}. {Title}

{FightCharacter.TotalDamageDone:N0} ({PercentOfMastersOrOwnTotalPlusPets.FormatPercent()}) total dmg
{PercentOfTotal.FormatPercent()} of fight's total dmg
{PercentOfMax.FormatPercent()} of fight's max dmg

{FightCharacter.WeaponDamageDonePM.Format()} ({FightCharacter.WeaponPercentOfTotalDamageDone.FormatPercent()}) weapon dmg / min
{FightCharacter.NanoDamageDonePM.Format()} ({FightCharacter.NanoPercentOfTotalDamageDone.FormatPercent()}) nano dmg / min
{FightCharacter.IndirectDamageDonePM.Format()} ({FightCharacter.IndirectPercentOfTotalDamageDone.FormatPercent()}) indirect dmg / min
{(!FightCharacter.HasCompleteAbsorbedDamageDoneStats ? "≥ " : "")}{FightCharacter.AbsorbedDamageDonePM.Format()} ({FightCharacter.AbsorbedPercentOfTotalDamageDone.FormatPercent()}) absorbed dmg / min
{FightCharacter.TotalDamageDonePM.Format()} total dmg / min

{(!FightCharacter.HasCompleteMissStats ? "≤ " : "")}{FightCharacter.WeaponHitDoneChance.FormatPercent()} weapon hit chance
  {FightCharacter.CritDoneChance.FormatPercent()} crit chance
  {FightCharacter.GlanceDoneChance.FormatPercent()} glance chance

{(!FightCharacter.HasCompleteMissStats ? "≥ " : "")}{FightCharacter.WeaponHitAttemptsDonePM.Format()} weapon hit attempts / min
{FightCharacter.WeaponHitsDonePM.Format()} weapon hits / min
  {FightCharacter.CritsDonePM.Format()} crits / min
  {FightCharacter.GlancesDonePM.Format()} glances / min
{FightCharacter.NanoHitsDonePM.Format()} nano hits / min
{FightCharacter.IndirectHitsDonePM.Format()} indirect hits / min
{(!FightCharacter.HasCompleteAbsorbedDamageDoneStats ? "≥ " : "")}{FightCharacter.AbsorbedHitsDonePM.Format()} absorbed hits / min
{FightCharacter.TotalHitsDonePM.Format()} total hits / min

{FightCharacter.AverageWeaponDamageDone.Format()} weapon dmg / hit
  {FightCharacter.AverageCritDamageDone.Format()} crit dmg / hit
  {FightCharacter.AverageGlanceDamageDone.Format()} glance dmg / hit
{FightCharacter.AverageNanoDamageDone.Format()} nano dmg / hit
{FightCharacter.AverageIndirectDamageDone.Format()} indirect dmg / hit
{FightCharacter.AverageAbsorbedDamageDone.Format()} absorbed dmg / hit"
+ (!FightCharacter.HasSpecialsDone ? null : $@"

{FightCharacter.GetSpecialsDoneInfo()}")
+ (FightCharacter.TotalDamageDone == 0 ? null : $@"

{FightCharacter.GetDamageTypesDoneInfo()}")
+ (FightCharacter.HealthDrained == 0 ? null : $@"

{FightCharacter.HealthDrainedPM.Format()} health drained / min
{FightCharacter.NanoDrainedPM.Format()} nano drained / min");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentOfTotal = Settings.Default.IncludeTopLevelNPCRows
                ? FightCharacter.PercentOfFightsTotalDamageDone
                : FightCharacter.PercentOfFightsTotalPlayerDamageDonePlusPets;
            PercentOfMax = Settings.Default.IncludeTopLevelNPCRows
                ? FightCharacter.PercentOfFightsMaxDamageDonePlusPets
                : FightCharacter.PercentOfFightsMaxPlayerDamageDonePlusPets;
            PercentOfMastersOrOwnTotalPlusPets = FightCharacter.PercentOfMastersOrOwnTotalDamageDonePlusPets;
            RightText = $"{FightCharacter.TotalDamageDone.Format()} ({FightCharacter.TotalDamageDonePM.Format()}, {PercentOfMastersOrOwnTotalPlusPets.FormatPercent()}, {DisplayedPercent.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
