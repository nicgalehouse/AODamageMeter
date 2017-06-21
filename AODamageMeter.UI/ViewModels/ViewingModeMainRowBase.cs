using AODamageMeter.UI.ViewModels.Rows;
using System.Collections.Generic;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class ViewingModeMainRowBase : MainRowBase
    {
        public static IReadOnlyList<ViewingModeMainRowBase> GetRows(FightViewModel fightViewModel)
            => new ViewingModeMainRowBase[]
            {
                new DamageDoneViewingModeMainRow(fightViewModel),
                new DamageTakenViewingModeMainRow(fightViewModel),
                new OwnersHealingDoneViewingModeMainRow(fightViewModel),
                new OwnersHealingTakenViewingModeMainRow(fightViewModel),
                new OwnersCastsViewingModeMainRow(fightViewModel),
                new OwnersXPViewingModeMainRow(fightViewModel)
            };

        protected ViewingModeMainRowBase(ViewingMode viewingMode,
            string title, int displayIndex, string iconPath, Color color, FightViewModel fightViewModel)
            : base(fightViewModel)
        {
            ViewingMode = viewingMode;
            Title = title;
            DisplayIndex = displayIndex;
            IconPath = iconPath;
            Color = color;
            PercentWidth = 1;
        }

        public ViewingMode ViewingMode { get; }

        public sealed override string Title { get; }
        public sealed override string UnnumberedLeftText => Title;
        public sealed override bool SupportsRowNumbers => false;
    }
}
