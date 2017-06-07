using AODamageMeter.UI.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class MainRowBase : RowBase
    {
        protected MainRowBase(FightCharacter fightCharacter = null)
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

        protected readonly Dictionary<FightCharacter, DetailRowBase> _detailRowMap = new Dictionary<FightCharacter, DetailRowBase>();
        public ObservableCollection<DetailRowBase> DetailRows { get; } = new ObservableCollection<DetailRowBase>();

        public override void Update(int? displayIndex = null)
        {
            IconPath = FightCharacter.Profession.GetIconPath();
            Color = FightCharacter.Profession.GetColor();

            base.Update(displayIndex);
        }
    }
}
