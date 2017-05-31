using AODamageMeter.UI.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class MainRowViewModelBase : RowViewModelBase
    {
        public MainRowViewModelBase(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        public bool CanShowDetails => DetailRows.Any();

        private bool _showDetails;
        public bool ShowDetails
        {
            get => _showDetails;
            private set => Set(ref _showDetails, value);
        }

        public void TryTogglingShowDetails()
            => ShowDetails = CanShowDetails ? !ShowDetails : false;

        protected readonly Dictionary<FightCharacter, RowViewModelBase> _detailRowMap = new Dictionary<FightCharacter, RowViewModelBase>();
        public ObservableCollection<RowViewModelBase> DetailRows { get; } = new ObservableCollection<RowViewModelBase>();

        public override void Update(int displayIndex)
        {
            IconPath = FightCharacter.Profession.GetIconPath();
            Color = FightCharacter.Profession.GetColor();

            base.Update(displayIndex);
        }
    }
}
