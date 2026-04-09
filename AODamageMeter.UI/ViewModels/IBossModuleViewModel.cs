using AODamageMeter.UI.ViewModels.BossModules;
using System.Collections.ObjectModel;

namespace AODamageMeter.UI.ViewModels
{
    public interface IBossModuleViewModel
    {
        string IconPath { get; }
        void OnFightEventAdded(FightEvent fightEvent);
        void UpdateView();
        bool IsPaused { get; set; }
        void Reset();
        ObservableCollection<StatusBarViewModel> StatusBars { get; }
        bool HasStatusBars { get; }
    }
}
