using AODamageMeter.UI.Helpers;
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
                new DamageDoneViewingModeMainRow(fight)
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

        public ViewingMode ViewingMode { get; }
        public sealed override string LeftText { get; }

        public Fight Fight { get; }

        public override string LeftTextToolTip
        {
            get
            {
                lock (Fight.DamageMeter)
                {
                    string professionsInfo = Fight.PlayerCount != 0 ?
$@"

{Fight.GetProfessionsInfo()}" : null;

                    return
$@"{Fight.FightCharacterCount} {(Fight.FightCharacterCount == 1 ? "character" : "characters")}
  {Fight.PlayerCount} {(Fight.PlayerCount == 1 ? "player" : "players")}
  {Fight.NPCCount} {(Fight.NPCCount == 1 ? "NPC" : "NPCs")}

{Fight.AveragePlayerLevel.Format()} avg level
{Fight.AverageAlienLevel.Format()} avg alien level{professionsInfo}";
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
