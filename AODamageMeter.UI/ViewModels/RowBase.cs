using AODamageMeter.UI.Helpers;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class RowBase : ViewModelBase
    {
        protected const string EmDash = NumberFormatter.EmDash;

        protected RowBase(FightCharacter fightCharacter = null)
            => FightCharacter = fightCharacter;

        public FightCharacter FightCharacter { get; }
        public string FightCharacterName => FightCharacter?.UncoloredName;

        public virtual string LeftText => FightCharacterName;

        protected int _displayIndex;
        public int DisplayIndex
        {
            get => _displayIndex;
            protected set => Set(ref _displayIndex, value);
        }

        protected string _iconPath;
        public string IconPath
        {
            get => _iconPath;
            protected set => Set(ref _iconPath, value);
        }

        protected Color _color;
        public Color Color
        {
            get => _color;
            protected set => Set(ref _color, value);
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

        public virtual bool IsLeftTextToolTipEnabled => true;
        public virtual string LeftTextToolTip
            => $"{DisplayIndex}. {FightCharacterName}"
+ (!FightCharacter.HasPlayerInfo ? null : $@"
{FightCharacter.Level}/{FightCharacter.AlienLevel} {FightCharacter.Faction} {FightCharacter.Profession}
{FightCharacter.Breed} {FightCharacter.Gender}")
+ (!FightCharacter.HasOrganizationInfo ? null : $@"
{FightCharacter.Organization} ({FightCharacter.OrganizationRank})");

        public virtual bool IsRightTextTooltipEnabled => true;
        public abstract string RightTextToolTip { get; }

        public virtual void Update(int? displayIndex = null)
        {
            DisplayIndex = displayIndex ?? DisplayIndex;
            RaisePropertyChanged(nameof(LeftTextToolTip));
            RaisePropertyChanged(nameof(RightTextToolTip));
        }
    }
}
