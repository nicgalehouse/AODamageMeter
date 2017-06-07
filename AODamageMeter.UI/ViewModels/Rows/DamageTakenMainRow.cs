using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageTakenMainRow : MainRowBase
    {
        public DamageTakenMainRow(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        public override string RightTextToolTip
        {
            get
            {
                lock (FightCharacter.DamageMeter)
                {
                    string specialsTakenInfo = FightCharacter.HasSpecialsTaken ?
$@"

{FightCharacter.GetSpecialsTakenInfo()}" : null;

                    return
$@"{DisplayIndex}. {FightCharacterName}

{FightCharacter.WeaponHitTakenChance.FormatPercent()} weapon hit chance
  {FightCharacter.CritTakenChance.FormatPercent()} crit chance
  {FightCharacter.GlanceTakenChance.FormatPercent()} glance chance

{FightCharacter.WeaponHitAttemptsTakenPM.Format()} weapon hit attempts / min
  {FightCharacter.WeaponHitsTakenPM.Format()} weapon hits / min
  {FightCharacter.CritsTakenPM.Format()} crits / min
  {FightCharacter.GlancesTakenPM.Format()} glances / min
{FightCharacter.NanoHitsTakenPM.Format()} nano hits / min
{FightCharacter.IndirectHitsTakenPM.Format()} indirect hits / min
{FightCharacter.TotalHitsTakenPM.Format()} total hits / min

{FightCharacter.WeaponDamageTakenPM.Format()} ({FightCharacter.WeaponPercentOfTotalDamageTaken.FormatPercent()}) weapon dmg / min
{FightCharacter.NanoDamageTakenPM.Format()} ({FightCharacter.NanoPercentOfTotalDamageTaken.FormatPercent()}) nano dmg / min
{FightCharacter.IndirectDamageTakenPM.Format()} ({FightCharacter.IndirectPercentOfTotalDamageTaken.FormatPercent()}) indirect dmg / min
{FightCharacter.TotalDamageTakenPM.Format()} total dmg / min

{FightCharacter.AverageWeaponDamageTaken.Format()} weapon dmg / hit
  {FightCharacter.AverageCritDamageTaken.Format()} crit dmg / hit
  {FightCharacter.AverageGlanceDamageTaken.Format()} glance dmg / hit
{FightCharacter.AverageNanoDamageTaken.Format()} nano dmg / hit
{FightCharacter.AverageIndirectDamageTaken.Format()} indirect dmg / hit{specialsTakenInfo}";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentWidth = FightCharacter.PercentOfFightsMaxDamageTaken ?? 0;
            double? percentTaken = Settings.Default.ShowPercentOfTotal
                ? FightCharacter.PercentOfFightsTotalDamageTaken : FightCharacter.PercentOfFightsMaxDamageTaken;
            RightText = $"{FightCharacter.TotalDamageTaken.Format()} ({FightCharacter.TotalDamageTakenPM.Format()}, {percentTaken.FormatPercent()})";

            base.Update(displayIndex);
        }
    }
}
