using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using AODamageMeter.UI.ViewModels.Rows;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class ViewingModeMainRowBase : MainRowBase
    {
        public static IReadOnlyList<ViewingModeMainRowBase> GetRows(Fight fight)
            => new ViewingModeMainRowBase[]
            {
                new DamageDoneViewingModeMainRow(fight),
                new DamageTakenViewingModeMainRow(fight),
            };

        protected ViewingModeMainRowBase(ViewingMode viewingMode,
            string leftText, int displayIndex, string iconPath, Color color, Fight fight)
        {
            ViewingMode = viewingMode;
            LeftText = leftText;
            DisplayIndex = displayIndex;
            IconPath = iconPath;
            Color = color;
            PercentWidth = 1;
            Fight = fight;
        }

        public Fight Fight { get; }

        public ViewingMode ViewingMode { get; }
        public sealed override string LeftText { get; }

        public override string LeftTextToolTip
        {
            get
            {
                lock (Fight.DamageMeter)
                {
                    var counts = ViewingMode == ViewingMode.DamageDone ? Fight.GetFightCharacterCounts(
                            includeNPCs: Settings.Default.ShowTopLevelNPCRows,
                            includeZeroDamageDones: Settings.Default.ShowTopLevelZeroDamageRows)
                        : ViewingMode == ViewingMode.DamageTaken ? Fight.GetFightCharacterCounts(
                            includeNPCs: Settings.Default.ShowTopLevelNPCRows,
                            includeZeroDamageTakens: Settings.Default.ShowTopLevelZeroDamageRows)
                        : throw new NotImplementedException();

                    return
$@"{counts.FightCharacterCount} {(counts.FightCharacterCount == 1 ? "character" : "characters")}
  {counts.PlayerCount} {(counts.PlayerCount == 1 ? "player" : "players")}, {counts.AveragePlayerLevel.Format()}/{counts.AveragePlayerAlienLevel.Format()}
    {counts.OmniPlayerCount} {(counts.OmniPlayerCount == 1 ? "Omni" : "Omnis")}, {counts.AverageOmniPlayerLevel.Format()}/{counts.AverageOmniPlayerAlienLevel.Format()}
    {counts.ClanPlayerCount} {(counts.ClanPlayerCount == 1 ? "Clan" : "Clan")}, {counts.AverageClanPlayerLevel.Format()}/{counts.AverageClanPlayerAlienLevel.Format()}
    {counts.NeutralPlayerCount} {(counts.NeutralPlayerCount == 1 ? "Neutral" : "Neutrals")}, {counts.AverageNeutralPlayerLevel.Format()}/{counts.AverageNeutralPlayerAlienLevel.Format()}"
+ (counts.UnknownPlayerCount == 0 ? null : $@"
    {counts.UnknownPlayerCount} {(counts.UnknownPlayerCount == 1 ? "Unknown" : "Unknowns")}, {counts.AverageUnknownPlayerLevel.Format()}/{counts.AverageUnknownPlayerAlienLevel.Format()}")
+ (counts.PetCount == 0 ? null : $@"
  {counts.PetCount} {(counts.PetCount == 1 ? "pet" : "pets")}")
+ (counts.NPCCount == 0 ? null : $@"
  {counts.NPCCount} {(counts.NPCCount == 1 ? "NPC" : "NPCs")}")
+ (counts.PlayerCount == 0 ? null : $@"

{counts.GetProfessionsInfo()}");
                }
            }
        }

        public override void Update(int? displayIndex = null)
        {
            RaisePropertyChanged(nameof(LeftTextToolTip));
            RaisePropertyChanged(nameof(RightTextToolTip));
        }
    }
}
