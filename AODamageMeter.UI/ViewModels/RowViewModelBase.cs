using AODamageMeter.UI.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        protected Dictionary<FightCharacter, RowViewModelBase> _detailRowViewModelsMap = new Dictionary<FightCharacter, RowViewModelBase>();
        public ObservableCollection<RowViewModelBase> DetailRowViewModels { get; } = new ObservableCollection<RowViewModelBase>();

        public virtual void Update(int? displayIndex = null)
        {
            DisplayIndex = displayIndex;
            IconPath = FightCharacter.Profession.GetIconPath();
            Color = FightCharacter.Profession.GetColor();
        }
    }
}
