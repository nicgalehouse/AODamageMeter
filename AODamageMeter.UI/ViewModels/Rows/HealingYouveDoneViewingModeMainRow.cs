using System;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class HealingYouveDoneViewingModeMainRow : ViewingModeMainRowBase
    {
        public HealingYouveDoneViewingModeMainRow(Fight fight)
            : base(ViewingMode.HealingYouveDone, "Healing You've Done", 3, "/Icons/HealingYouveDone.png", Color.FromRgb(197, 135, 25), fight)
        { }

        public override string RightTextToolTip => throw new NotImplementedException();

        public override void Update(int? displayIndex = null)
        {

        }
    }
}
