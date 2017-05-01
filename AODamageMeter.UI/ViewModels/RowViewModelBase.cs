using AODamageMeter.UI.Helpers;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class RowViewModelBase : ViewModelBase
    {
        protected RowViewModelBase(FightCharacter fightCharacter)
        {
            FightCharacter = fightCharacter;
            Update();
        }

        public FightCharacter FightCharacter { get; }

        protected string _iconPath;
        public string IconPath
        {
            get => _iconPath;
            protected set => Set(ref _iconPath, value);
        }

        protected string _colorHexCode;
        public string ColorHexCode
        {
            get => _colorHexCode;
            protected set => Set(ref _colorHexCode, value);
        }

        protected double _percentWidth;
        public double PercentWidth
        {
            get => _percentWidth;
            protected set => Set(ref _percentWidth, value);
        }

        protected string _rightText;
        public string RightText
        {
            get => _rightText;
            protected set => Set(ref _rightText, value);
        }

        public virtual void Update()
        {
            IconPath = FightCharacter.Profession.GetIconPath();
            ColorHexCode = FightCharacter.Profession.GetColorHexCode();
        }
    }
}
