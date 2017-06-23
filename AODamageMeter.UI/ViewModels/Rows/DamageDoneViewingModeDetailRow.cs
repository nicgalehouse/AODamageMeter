using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageDoneViewingModeDetailRow : FightCharacterDetailRowBase
    {
        public DamageDoneViewingModeDetailRow(FightViewModel fightViewModel, FightCharacter fightCharacter)
            : base(fightViewModel, fightCharacter, showIcon: true)
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

            base.Update(displayIndex);
        }

        public override bool TryCopyAndScriptProgressedRowsInfo()
            => CopyAndScriptProgressedRowsInfo(FightViewModel.GetUpdatedDamageDoneInfoRows(FightCharacter));
    }
}
