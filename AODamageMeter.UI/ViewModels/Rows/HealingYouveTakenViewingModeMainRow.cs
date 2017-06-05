using System;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class HealingYouveTakenViewingModeMainRow : ViewingModeMainRowBase
    {
        public HealingYouveTakenViewingModeMainRow(Fight fight)
            : base(ViewingMode.HealingYouveTaken, "Healing You've Taken", 4, "/Icons/HealingYouveTaken.png", Color.FromRgb(184, 100, 57), fight)
        { }

        public override string RightTextToolTip => throw new NotImplementedException();

        public override void Update(int? displayIndex = null)
        {

        }
    }
}
