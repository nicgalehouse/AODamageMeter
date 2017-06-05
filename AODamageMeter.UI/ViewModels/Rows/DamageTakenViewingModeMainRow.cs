using System;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageTakenViewingModeMainRow : ViewingModeMainRowBase
    {
        public DamageTakenViewingModeMainRow(Fight fight)
            : base(ViewingMode.DamageTaken, "Damage Taken", 2, "/Icons/DamageTaken.png", Color.FromRgb(88, 166, 86), fight)
        { }

        public override string RightTextToolTip => throw new NotImplementedException();

        public override void Update(int? displayIndex = null)
        {

        }
    }
}
