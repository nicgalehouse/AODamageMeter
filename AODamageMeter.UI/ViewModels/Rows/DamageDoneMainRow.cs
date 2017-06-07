using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageDoneMainRow : MainRowBase
    {
        public DamageDoneMainRow(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        public override string RightTextToolTip
        {
            get
            {
                lock (FightCharacter.DamageMeter)
                {
                    string specialsDoneInfo = FightCharacter.HasSpecialsDone ?
$@"

{FightCharacter.GetSpecialsDoneInfo()}" : null;

                    return
$@"{DisplayIndex}. {FightCharacterName}

{FightCharacter.WeaponHitDoneChancePlusPets.FormatPercent()} weapon hit chance
  {FightCharacter.CritDoneChancePlusPets.FormatPercent()} crit chance
  {FightCharacter.GlanceDoneChancePlusPets.FormatPercent()} glance chance

{FightCharacter.WeaponHitAttemptsDonePMPlusPets.Format()} weapon hit attempts / min
  {FightCharacter.WeaponHitsDonePMPlusPets.Format()} weapon hits / min
  {FightCharacter.CritsDonePMPlusPets.Format()} crits / min
  {FightCharacter.GlancesDonePMPlusPets.Format()} glances / min
{FightCharacter.NanoHitsDonePMPlusPets.Format()} nano hits / min
{FightCharacter.IndirectHitsDonePMPlusPets.Format()} indirect hits / min
{FightCharacter.TotalHitsDonePMPlusPets.Format()} total hits / min

{FightCharacter.WeaponDamageDonePMPlusPets.Format()} ({FightCharacter.WeaponPercentOfTotalDamageDonePlusPets.FormatPercent()}) weapon dmg / min
{FightCharacter.NanoDamageDonePMPlusPets.Format()} ({FightCharacter.NanoPercentOfTotalDamageDonePlusPets.FormatPercent()}) nano dmg / min
{FightCharacter.IndirectDamageDonePMPlusPets.Format()} ({FightCharacter.IndirectPercentOfTotalDamageDonePlusPets.FormatPercent()}) indirect dmg / min
{FightCharacter.TotalDamageDonePMPlusPets.Format()} total dmg / min

{FightCharacter.AverageWeaponDamageDonePlusPets.Format()} weapon dmg / hit
  {FightCharacter.AverageCritDamageDonePlusPets.Format()} crit dmg / hit
  {FightCharacter.AverageGlanceDamageDonePlusPets.Format()} glance dmg / hit
{FightCharacter.AverageNanoDamageDonePlusPets.Format()} nano dmg / hit
{FightCharacter.AverageIndirectDamageDonePlusPets.Format()} indirect dmg / hit{specialsDoneInfo}";
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            if (!FightCharacter.IsFightPetOwner)
            {
                PercentWidth = FightCharacter.PercentOfFightsMaxDamageDonePlusPets ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? FightCharacter.PercentOfFightsTotalDamageDone : FightCharacter.PercentOfFightsMaxDamageDonePlusPets;
                RightText = $"{FightCharacter.TotalDamageDone.Format()} ({FightCharacter.TotalDamageDonePM.Format()}, {percentDone.FormatPercent()})";
            }
            else
            {
                PercentWidth = FightCharacter.PercentPlusPetsOfFightsMaxDamageDonePlusPets ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? FightCharacter.PercentPlusPetsOfFightsTotalDamageDone : FightCharacter.PercentPlusPetsOfFightsMaxDamageDonePlusPets;
                RightText = $"{FightCharacter.TotalDamageDonePlusPets.Format()} ({FightCharacter.TotalDamageDonePMPlusPets.Format()}, {percentDone.FormatPercent()})";

                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { FightCharacter }.Concat(FightCharacter.FightPets)
                    .OrderByDescending(c => c.TotalDamageDonePlusPets)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new DamageDoneDetailRow(fightCharacter));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            base.Update(displayIndex);
        }
    }
}
