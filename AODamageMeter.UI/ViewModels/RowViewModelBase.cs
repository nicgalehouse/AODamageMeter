using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class RowViewModelBase : ViewModelBase
    {
        protected RowViewModelBase(FightCharacter fightCharacter)
            => FightCharacter = fightCharacter;

        public FightCharacter FightCharacter { get; }
        public string FightCharacterName => FightCharacter.Name;

        protected int? _displayIndex;
        public int? DisplayIndex
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

        public string CharacterTooltip
        {
            get
            {
                string playerInfo = FightCharacter.HasPlayerInfo ?
$@"
{FightCharacter.Level}/{FightCharacter.AlienLevel} {FightCharacter.Faction} {FightCharacter.Profession}
{FightCharacter.Breed} {FightCharacter.Gender}" : null;

                string organizationInfo = FightCharacter.HasOrganizationInfo ?
$@"
{FightCharacter.Organization} ({FightCharacter.OrganizationRank})" : null;

                return $"{DisplayIndex}. {FightCharacterName}{playerInfo}{organizationInfo}";
            }
        }

        public abstract string RightTextToolTip { get; }

        public virtual void Update(int displayIndex)
        {
            DisplayIndex = displayIndex;
            RaisePropertyChanged(nameof(CharacterTooltip));
            RaisePropertyChanged(nameof(RightTextToolTip));
        }
    }
}
