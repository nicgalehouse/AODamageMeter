using System.Collections.ObjectModel;
using System.Linq;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class MainRowBase : RowBase
    {
        protected MainRowBase(DamageMeterViewModel damageMeterViewModel)
            : base(damageMeterViewModel)
        { }

        public bool CanShowDetails => DetailRows.Any();

        private bool _showDetails;
        public bool ShowDetails
        {
            get => _showDetails;
            private set => Set(ref _showDetails, value);
        }

        public void TryToggleShowDetails()
            => ShowDetails = CanShowDetails ? !ShowDetails : false;

        public ObservableCollection<DetailRowBase> DetailRows { get; } = new ObservableCollection<DetailRowBase>();
    }
}
