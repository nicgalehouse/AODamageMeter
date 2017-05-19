using AODamageMeter.UI.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class MainRowViewModelBase : RowViewModelBase
    {
        public MainRowViewModelBase(FightCharacter fightCharacter)
            : base(fightCharacter)
        { }

        protected Dictionary<FightCharacter, RowViewModelBase> _detailRowViewModelsMap = new Dictionary<FightCharacter, RowViewModelBase>();
        public ObservableCollection<RowViewModelBase> DetailRowViewModels { get; } = new ObservableCollection<RowViewModelBase>();

        public override void Update(int displayIndex)
        {
            IconPath = FightCharacter.Profession.GetIconPath();
            Color = FightCharacter.Profession.GetColor();

            base.Update(displayIndex);
        }
    }
}
