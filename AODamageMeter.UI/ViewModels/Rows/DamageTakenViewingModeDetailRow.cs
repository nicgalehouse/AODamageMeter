using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System.Linq;
using System.Text;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public sealed class DamageTakenViewingModeDetailRow : FightCharacterDetailRowBase
    {
        public DamageTakenViewingModeDetailRow(FightViewModel fightViewModel, FightCharacter fightCharacter)
            : base(fightViewModel, fightCharacter, showIcon: true)
        { }

        public override string Title => $"{FightCharacterName}'s Damage Taken";

        public override string RightTextToolTip
        {
            get
            {
                lock (Fight)
                {
                    return FightCharacter.GetFightCharacterDamageTakenTooltip(DisplayIndex);
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            PercentOfTotal = Settings.Default.IncludeTopLevelNPCRows
                ? FightCharacter.PercentOfFightsTotalDamageTaken
                : FightCharacter.PercentOfFightsTotalPlayerOrPetDamageTaken;
            PercentOfMax = Settings.Default.IncludeTopLevelNPCRows
                ? FightCharacter.PercentOfFightsMaxDamageTaken
                : FightCharacter.PercentOfFightsMaxPlayerOrPetDamageTaken;
            RightText = $"{FightCharacter.TotalDamageTaken.Format()} ({FightCharacter.TotalDamageTakenPM.Format()}, {DisplayedPercent.FormatPercent()})";

            base.Update(displayIndex);
        }

        public override bool TryCopyAndScriptProgressedRowsInfo()
        {
            var body = new StringBuilder();
            foreach (var damageTakenInfoRow in FightViewModel.GetUpdatedDamageTakenInfoRows(FightCharacter)
                .OrderBy(r => r.DisplayIndex))
            {
                body.AppendLine(damageTakenInfoRow.RowScriptText);
            }

            CopyAndScript(body.ToString());

            return true;
        }
    }
}
