using AODamageMeter.Buffs;
using AODamageMeter.FightEvents;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public abstract class BossModuleViewModelBase : ViewModelBase, IBossModuleViewModel
    {
        private readonly ConcurrentQueue<StatusBarViewModel> _pendingStatusBars
            = new ConcurrentQueue<StatusBarViewModel>();

        public abstract string BossName { get; }
        public abstract string IconPath { get; }
        public bool HasFightStarted { get; protected set; }
        public abstract void OnFightEventAdded(FightEvent fightEvent);
        public abstract void UpdateView();
        public abstract bool IsPaused { get; set; }
        public abstract void Reset();

        public ObservableCollection<StatusBarViewModel> StatusBars { get; }
            = new ObservableCollection<StatusBarViewModel>();

        public bool HasStatusBars => StatusBars.Count > 0;

        protected virtual void OnFightStarted() { }

        protected bool CheckForFightStart(FightEvent fightEvent)
        {
            if (!HasFightStarted
                && (fightEvent.Source?.Name == BossName || fightEvent.Target?.Name == BossName))
            {
                HasFightStarted = true;
                OnFightStarted();
            }

            return HasFightStarted;
        }

        protected void CheckImportantBuffs(FightEvent fightEvent)
        {
            if (!(fightEvent is MeCastNano castEvent)
                || castEvent.CastResult != CastResult.Success
                || castEvent.NanoProgram == null)
                return;

            if (TotalMirrorShield.Nanoline.TryGetBuff(castEvent.NanoProgram, out var buff))
            {
                RequestStatusBar(buff.ShortName, buff.DurationSeconds, buff.Color, buff.IconPath);
            }
        }

        protected void RequestStatusBar(string label, double totalSeconds, string color, string iconPath)
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
