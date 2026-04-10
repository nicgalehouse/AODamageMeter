using AODamageMeter.Buffs;
using AODamageMeter.FightEvents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AODamageMeter.UI.ViewModels
{
    public abstract class BossModuleViewModelBase : ViewModelBase
    {
        public abstract string BossName { get; }
        public abstract string IconPath { get; }

        public bool HasFightStarted { get; protected set; }

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                if (_isPaused == value) return;

                _isPaused = value;

                if (_isPaused)
                {
                    // Can't pause boss fights in-world, so for simplicity we just reset when paused.
                    Reset();
                }

                RaisePropertyChanged(nameof(IsPaused));
            }
        }

        private long? _lastManualNanoProgramDeactivationUnixSeconds;
        private DateTime? _lastManualNanoProgramDeactivationTimestamp;
        private readonly ConcurrentDictionary<string, long> _wipedNanoPrograms = new ConcurrentDictionary<string, long>();
        public IReadOnlyList<string> WipedNanoPrograms => _wipedNanoPrograms
            .OrderBy(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => kvp.Key)
            .ToList();
        public bool HasWipedNanoPrograms => !_wipedNanoPrograms.IsEmpty;

        private readonly ConcurrentQueue<StatusBarViewModel> _pendingStatusBars = new ConcurrentQueue<StatusBarViewModel>();
        public ObservableCollection<StatusBarViewModel> StatusBars { get; } = new ObservableCollection<StatusBarViewModel>();
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

        public virtual void OnFightEventAdded(FightEvent fightEvent)
        {
            if (HasFightStarted || CheckForFightStart(fightEvent))
            {
                CheckNcuWipes(fightEvent);
                CheckStatusBars(fightEvent);
            }
        }

        // We add nano programs that get cancelled as part of a recast, but then immediately remove
        // them once the recast is proven. So it should be okay--there shouldn't be any UI flicker.
        // One thing we can't easily do is recognize when nanoprograms are terminated by being
        // overwritten by a better nanoprogram. Ideally we wouldn't show those terminations.
        private void CheckNcuWipes(FightEvent fightEvent)
        {
            if (fightEvent is SystemEvent systemEvent)
            {
                if (systemEvent.IsNanoDeactivated)
                {
                    _lastManualNanoProgramDeactivationUnixSeconds = fightEvent.LogUnixSeconds;
                    _lastManualNanoProgramDeactivationTimestamp = fightEvent.Timestamp;
                }
                else if (systemEvent.IsNanoTerminated)
                {
                    // Manual deactivations can come in a bit before the actual termination.
                    bool isPurposeful = _lastManualNanoProgramDeactivationUnixSeconds == fightEvent.LogUnixSeconds
                        || _lastManualNanoProgramDeactivationTimestamp?.AddSeconds(.5) >= fightEvent.Timestamp;
                    _lastManualNanoProgramDeactivationUnixSeconds = null;
                    _lastManualNanoProgramDeactivationTimestamp = null;

                    if (!isPurposeful)
                    {
                        _wipedNanoPrograms.TryAdd(systemEvent.NanoProgram, fightEvent.LogUnixSeconds);
                    }
                }
                else if (systemEvent.IsFriendlyNanoExecutedOnYou)
                {
                    _wipedNanoPrograms.TryRemove(systemEvent.NanoProgram, out _);
                }
            }
            else if (fightEvent is MeCastNano castEvent
                && castEvent.CastResult == CastResult.Success
                && castEvent.NanoProgram != null)
            {
                _wipedNanoPrograms.TryRemove(castEvent.NanoProgram, out _);
            }
        }

        protected virtual void CheckStatusBars(FightEvent fightEvent)
        {
            if (!(fightEvent is MeCastNano castEvent)
                || castEvent.CastResult != CastResult.Success
                || castEvent.NanoProgram == null)
                return;

            if (TotalMirrorShield.Nanoline.TryGetBuff(castEvent.NanoProgram, out var buff)
                || NullitySphere.Nanoline.TryGetBuff(castEvent.NanoProgram, out buff))
            {
                RequestStatusBar(buff.ShortName, buff.DurationSeconds, buff.Color, buff.IconPath);
            }
        }

        protected void RequestStatusBar(string label, double totalSeconds, string color, string iconPath)
            => _pendingStatusBars.Enqueue(new StatusBarViewModel(label, totalSeconds, color, iconPath));

        public virtual void UpdateView()
        {
            if (!HasFightStarted)
                return;

            UpdateNcuWipes();
            UpdateStatusBars();
        }

        private void UpdateNcuWipes()
        {
            RaisePropertyChanged(nameof(WipedNanoPrograms));
            RaisePropertyChanged(nameof(HasWipedNanoPrograms));
        }

        private void UpdateStatusBars()
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

        public virtual void Reset()
        {
            HasFightStarted = false;

            ResetNcuWipes();
            ResetStatusBars();
        }

        private void ResetNcuWipes()
        {
            _lastManualNanoProgramDeactivationUnixSeconds = null;
            _lastManualNanoProgramDeactivationTimestamp = null;
            _wipedNanoPrograms.Clear();

            RaisePropertyChanged(nameof(WipedNanoPrograms));
            RaisePropertyChanged(nameof(HasWipedNanoPrograms));
        }

        private void ResetStatusBars()
        {
            StatusBars.Clear();
            while (_pendingStatusBars.TryDequeue(out _)) { }

            RaisePropertyChanged(nameof(HasStatusBars));
        }
    }
}
