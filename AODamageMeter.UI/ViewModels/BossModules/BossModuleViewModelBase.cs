using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public abstract class BossModuleViewModelBase : ViewModelBase, IBossModuleViewModel
    {
        private readonly ConcurrentQueue<StatusBarViewModel> _pendingStatusBars
            = new ConcurrentQueue<StatusBarViewModel>();

        public abstract string IconPath { get; }
        public abstract void OnFightEventAdded(FightEvent fightEvent);
        public abstract void UpdateView();
        public abstract bool IsPaused { get; set; }
        public abstract void Reset();

        public ObservableCollection<StatusBarViewModel> StatusBars { get; }
            = new ObservableCollection<StatusBarViewModel>();

        public bool HasStatusBars => StatusBars.Count > 0;

        protected void RequestStatusBar(string label, double totalSeconds, Brush color, string iconPath)
            => _pendingStatusBars.Enqueue(new StatusBarViewModel(label, totalSeconds, color, iconPath));

        protected void UpdateStatusBars()
        {
            bool statusBarsCollectionChanged = false;

            while (_pendingStatusBars.TryDequeue(out var pendingStatusBar))
            {
                var existingStatusBar = StatusBars.FirstOrDefault(b => b.Key == pendingStatusBar.Key);

                if (existingStatusBar != null)
                {
                    StatusBars[StatusBars.IndexOf(existingStatusBar)] = pendingStatusBar;
                }
                else
                {
                    StatusBars.Add(pendingStatusBar);
                    statusBarsCollectionChanged = true;
                }
            }

            for (int i = StatusBars.Count - 1; i >= 0; i--)
            {
                var statusBar = StatusBars[i];
                statusBar.Refresh();

                if (statusBar.IsExpired)
                {
                    StatusBars.RemoveAt(i);
                    statusBarsCollectionChanged = true;
                }
            }

            if (statusBarsCollectionChanged)
            {
                RaisePropertyChanged(nameof(HasStatusBars));
            }
        }

        protected void ResetStatusBars()
        {
            bool hadStatusBars = StatusBars.Count > 0;
            StatusBars.Clear();

            while (_pendingStatusBars.TryDequeue(out _)) { }

            if (hadStatusBars)
            {
                RaisePropertyChanged(nameof(HasStatusBars));
            }
        }
    }
}
