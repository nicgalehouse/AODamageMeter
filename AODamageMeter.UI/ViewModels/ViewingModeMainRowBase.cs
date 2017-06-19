using AODamageMeter.UI.ViewModels.Rows;
using System.Collections.Generic;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class ViewingModeMainRowBase : MainRowBase
    {
        public static IReadOnlyList<ViewingModeMainRowBase> GetRows(DamageMeterViewModel damageMeterViewModel, Fight fight)
            => new ViewingModeMainRowBase[]
            {
                new DamageDoneViewingModeMainRow(damageMeterViewModel, fight),
                new DamageTakenViewingModeMainRow(damageMeterViewModel, fight),
                new OwnersHealingDoneViewingModeMainRow(damageMeterViewModel, fight),
                new OwnersHealingTakenViewingModeMainRow(damageMeterViewModel, fight),
                new OwnersCastsViewingModeMainRow(damageMeterViewModel, fight),
                new OwnersXPViewingModeMainRow(damageMeterViewModel, fight)
            };

        protected ViewingModeMainRowBase(ViewingMode viewingMode,
            string unnumberedLeftText, int displayIndex, string iconPath, Color color, DamageMeterViewModel damageMeterViewModel, Fight fight)
            : base(damageMeterViewModel)
        {
            ViewingMode = viewingMode;
            UnnumberedLeftText = unnumberedLeftText;
            DisplayIndex = displayIndex;
            IconPath = iconPath;
            Color = color;
            PercentWidth = 1;
            Fight = fight;
        }

        public Fight Fight { get; }
        public ViewingMode ViewingMode { get; }

        public sealed override string Title => UnnumberedLeftText;

        public sealed override string UnnumberedLeftText { get; }

        public sealed override bool SupportsRowNumbers => false;
    }
}
