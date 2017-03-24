using Prism.Mvvm;
using System.Drawing;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class Row : BindableBase
    {
        string _leftText;
        public string LeftText
        {
            get { return _leftText; }
            set { SetProperty(ref _leftText, value); }
        }

        Bitmap _icon;
        public Bitmap Icon
        {
            get
            { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        string _color;
        public string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }

        double _width;
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        string _rightText;
        public string RightText
        {
            get
            { return _rightText; }
            set
            { SetProperty(ref _rightText, value); }
        }

        public abstract void Update(FightCharacter character);
    }
}
