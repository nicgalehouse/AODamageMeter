using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;
using System.Text;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageDoneMainRow : FightCharacterMainRowBase
    {
        public DamageDoneMainRow(FightViewModel fightViewModel, FightCharacter fightCharacter)
            : base(fightViewModel, fightCharacter)
        { }

        public override string Title => $"{FightCharacterName}'s Damage Done";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return
$@"{DisplayIndex}. {FightCharacterName}

{FightCharacter.TotalDamageDonePlusPets:N0} total dmg

{FightCharacter.WeaponDamageDonePMPlusPets.Format()} ({FightCharacter.WeaponPercentOfTotalDamageDonePlusPets.FormatPercent()}) weapon dmg / min
{FightCharacter.NanoDamageDonePMPlusPets.Format()} ({FightCharacter.NanoPercentOfTotalDamageDonePlusPets.FormatPercent()}) nano dmg / min
{FightCharacter.IndirectDamageDonePMPlusPets.Format()} ({FightCharacter.IndirectPercentOfTotalDamageDonePlusPets.FormatPercent()}) indirect dmg / min
{FightCharacter.TotalDamageDonePMPlusPets.Format()} total dmg / min

{(FightCharacter.HasIncompleteMissStatsPlusPets ? "≤ " : "")}{FightCharacter.WeaponHitDoneChancePlusPets.FormatPercent()} weapon hit chance
  {FightCharacter.CritDoneChancePlusPets.FormatPercent()} crit chance
  {FightCharacter.GlanceDoneChancePlusPets.FormatPercent()} glance chance

{(FightCharacter.HasIncompleteMissStatsPlusPets ? "≥ " : "")}{FightCharacter.WeaponHitAttemptsDonePMPlusPets.Format()} weapon hit attempts / min
{FightCharacter.WeaponHitsDonePMPlusPets.Format()} weapon hits / min
  {FightCharacter.CritsDonePMPlusPets.Format()} crits / min
  {FightCharacter.GlancesDonePMPlusPets.Format()} glances / min
{FightCharacter.NanoHitsDonePMPlusPets.Format()} nano hits / min
{FightCharacter.IndirectHitsDonePMPlusPets.Format()} indirect hits / min
{FightCharacter.TotalHitsDonePMPlusPets.Format()} total hits / min

{FightCharacter.AverageWeaponDamageDonePlusPets.Format()} weapon dmg / hit
  {FightCharacter.AverageCritDamageDonePlusPets.Format()} crit dmg / hit
  {FightCharacter.AverageGlanceDamageDonePlusPets.Format()} glance dmg / hit
{FightCharacter.AverageNanoDamageDonePlusPets.Format()} nano dmg / hit
{FightCharacter.AverageIndirectDamageDonePlusPets.Format()} indirect dmg / hit"
+ (!FightCharacter.HasSpecialsDone ? null : $@"

{FightCharacter.GetSpecialsDoneInfo()}");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            bool showNPCs = Settings.Default.ShowTopLevelNPCRows;

            if (!FightCharacter.IsFightPetOwner)
            {
                PercentWidth = (showNPCs
                    ? FightCharacter.PercentOfFightsMaxDamageDonePlusPets
                    : FightCharacter.PercentOfFightsMaxPlayerDamageDonePlusPets) ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? (showNPCs
                        ? FightCharacter.PercentOfFightsTotalDamageDone
                        : FightCharacter.PercentOfFightsTotalPlayerDamageDonePlusPets)
                    : (showNPCs
                        ? FightCharacter.PercentOfFightsMaxDamageDonePlusPets
                        : FightCharacter.PercentOfFightsMaxPlayerDamageDonePlusPets);
                RightText = $"{FightCharacter.TotalDamageDone.Format()} ({FightCharacter.TotalDamageDonePM.Format()}, {percentDone.FormatPercent()})";
            }
            else
            {
                PercentWidth = (showNPCs
                    ? FightCharacter.PercentPlusPetsOfFightsMaxDamageDonePlusPets
                    : FightCharacter.PercentPlusPetsOfFightsMaxPlayerDamageDonePlusPets) ?? 0;
                double? percentDone = Settings.Default.ShowPercentOfTotal
                    ? (showNPCs
                        ? FightCharacter.PercentPlusPetsOfFightsTotalDamageDone
                        : FightCharacter.PercentPlusPetsOfFightsTotalPlayerDamageDonePlusPets)
                    : (showNPCs
                        ? FightCharacter.PercentPlusPetsOfFightsMaxDamageDonePlusPets
                        : FightCharacter.PercentPlusPetsOfFightsMaxPlayerDamageDonePlusPets);
                RightText = $"{FightCharacter.TotalDamageDonePlusPets.Format()} ({FightCharacter.TotalDamageDonePMPlusPets.Format()}, {percentDone.FormatPercent()})";

                int detailRowDisplayIndex = 1;
                foreach (var fightCharacter in new[] { FightCharacter }.Concat(FightCharacter.FightPets)
                    .OrderByDescending(c => c.TotalDamageDone)
                    .ThenBy(c => c.UncoloredName))
                {
                    if (!_detailRowMap.TryGetValue(fightCharacter, out DetailRowBase detailRow))
                    {
                        _detailRowMap.Add(fightCharacter, detailRow = new DamageDoneDetailRow(FightViewModel, fightCharacter));
                        DetailRows.Add(detailRow);
                    }
                    detailRow.Update(detailRowDisplayIndex++);
                }
            }

            base.Update(displayIndex);
        }

        public override bool TryCopyAndScriptProgressedRowsInfo()
        {
            var body = new StringBuilder();
            foreach (var damageDoneInfoRow in FightViewModel.GetUpdatedDamageDoneInfoRows(FightCharacter)
                .OrderBy(r => r.DisplayIndex))
            {
                body.AppendLine(damageDoneInfoRow.RowScriptText);
            }

            CopyAndScript(body.ToString());

            return true;
        }
    }
}
