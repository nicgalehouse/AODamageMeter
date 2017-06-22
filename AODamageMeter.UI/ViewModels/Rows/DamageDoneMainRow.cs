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
                    return FightCharacter.GetFightCharacterDamageDoneTooltip(DisplayIndex, PercentOfTotal, PercentOfMax);
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentOfTotal = Settings.Default.IncludeTopLevelNPCRows
                ? FightCharacter.PercentPlusPetsOfFightsTotalDamageDone
                : FightCharacter.PercentPlusPetsOfFightsTotalPlayerDamageDonePlusPets;
            PercentOfMax = Settings.Default.IncludeTopLevelNPCRows
                ? FightCharacter.PercentPlusPetsOfFightsMaxDamageDonePlusPets
                : FightCharacter.PercentPlusPetsOfFightsMaxPlayerDamageDonePlusPets;
            RightText = $"{FightCharacter.TotalDamageDonePlusPets.Format()} ({FightCharacter.TotalDamageDonePMPlusPets.Format()}, {DisplayedPercent.FormatPercent()})";

            if (FightCharacter.IsFightPetOwner)
            {
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
