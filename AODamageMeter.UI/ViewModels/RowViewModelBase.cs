namespace AODamageMeter.UI.ViewModels
{
    public abstract class RowViewModelBase : ViewModelBase
    {
        protected FightCharacter _fightCharacter;

        protected RowViewModelBase(FightCharacter fightCharacter)
        {
            _fightCharacter = fightCharacter;
            Update();
        }

        public string Name => _fightCharacter.Name;

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

        protected double _width;
        public double Width
        {
            get => _width;
            protected set => Set(ref _width, value);
        }

        protected string _rightText;
        public string RightText
        {
            get => _rightText;
            protected set => Set(ref _rightText, value);
        }

        public abstract void Update();
    }
}
