using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;

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
                    return FightCharacter.GetFightCharacterDamageDoneTooltip(Title, DisplayIndex, PercentOfTotal, PercentOfMax);
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

            if (FightCharacter.IsFightPetMaster)
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

            CleanUpOldPetDetailRowsIfNecessary(FightCharacter);

            base.Update(displayIndex);
        }

        public override bool TryCopyAndScriptProgressedRowsInfo()
            => CopyAndScriptProgressedRowsInfo(FightViewModel.GetUpdatedDamageDoneInfoRows(FightCharacter));
    }
}
