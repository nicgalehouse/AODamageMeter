using System.Drawing;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class RowViewModelBase : ViewModelBase
    {
        protected string _leftText;
        public virtual string LeftText
        {
            get => _leftText;
            set => Set(ref _leftText, value);
        }

        protected Bitmap _icon;
        public virtual Bitmap Icon
        {
            get => _icon;
            set => Set(ref _icon, value);
        }

        protected string _color;
        public virtual string Color
        {
            get => _color;
            set => Set(ref _color, value);
        }

        protected double _width;
        public virtual double Width
        {
            get => _width;
            set => Set(ref _width, value);
        }

        protected string _rightText;
        public virtual string RightText
        {
            get => _rightText;
            set => Set(ref _rightText, value);
        }

        public abstract void Update(FightCharacter character);
    }
}
