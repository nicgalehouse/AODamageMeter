using AODamageMeter.FightEvents;
using AODamageMeter.Nanolines;
using AODamageMeter.UI.Helpers;
using AODamageMeter.UI.Properties;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public class TheBeastDualLoggedModuleViewModel : TheBeastModuleViewModel
    {
        private readonly object _secondaryFightLock = new object();
        private readonly DamageMeter _secondaryDamageMeter;
        private readonly Stopwatch _timeSinceSecondaryUpdate = new Stopwatch();
        private long _primaryFightStartUnixSeconds = long.MaxValue;
        private long _latestPrimaryFightEventUnixSeconds;
        private long _latestSecondaryFightEventUnixSeconds;
        private Fight _secondaryFight;

        public string PrimaryCharacterName => Settings.Default.SelectedCharacterName;
        public string SecondaryCharacterName { get; }

        private long? _secondaryLastManualNanoProgramDeactivationUnixSeconds;
        private DateTime? _secondaryLastManualNanoProgramDeactivationTimestamp;
        private readonly SynchronizedStopwatch _timeSinceSecondaryNcuWiped = new SynchronizedStopwatch();
        private readonly ConcurrentDictionary<string, long> _secondaryWipedNanoPrograms = new ConcurrentDictionary<string, long>();
        public IReadOnlyList<string> SecondaryWipedNanoPrograms => _secondaryWipedNanoPrograms
            .OrderBy(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => kvp.Key)
            .ToList();
        private readonly ConcurrentDictionary<string, long> _secondaryRecentlyWipedNanoPrograms = new ConcurrentDictionary<string, long>();
        public IReadOnlyList<string> SecondaryRecentlyWipedNanoPrograms => _secondaryRecentlyWipedNanoPrograms
            .OrderBy(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => kvp.Key)
            .ToList();
        public bool HasSecondaryWipedNanoPrograms => !_secondaryWipedNanoPrograms.IsEmpty;
        public bool IsSecondaryNcuWipeRecent { get; private set; }

        public TheBeastDualLoggedModuleViewModel(
            string secondaryCharacterName,
            Dimension secondaryDimension,
            string secondaryLogFilePath,
            DamageMeterMode mode = DamageMeterMode.Live)
        {
            SecondaryCharacterName = secondaryCharacterName;
            _secondaryDamageMeter = new DamageMeter(secondaryCharacterName, secondaryDimension, secondaryLogFilePath, mode);
        }

        protected override void OnFightStarted(FightEvent fightEvent)
        {
            base.OnFightStarted(fightEvent);

            lock (_secondaryFightLock)
            {
                _primaryFightStartUnixSeconds = fightEvent.LogUnixSeconds;

                if (_secondaryFight != null)
                {
                    _secondaryFight.FightEventAdded -= OnSecondaryFightEventAdded;
                }

                _secondaryDamageMeter.InitializeNewFight();
                _secondaryFight = _secondaryDamageMeter.CurrentFight;
                _secondaryFight.FightEventAdded += OnSecondaryFightEventAdded;
            }
        }

        public override void OnFightEventAdded(FightEvent fightEvent)
        {
            base.OnFightEventAdded(fightEvent);

            if (!HasFightStarted)
                return;

            _latestPrimaryFightEventUnixSeconds = fightEvent.LogUnixSeconds;

            if (!_timeSinceSecondaryUpdate.IsRunning || _timeSinceSecondaryUpdate.ElapsedMilliseconds >= 100)
            {
                _secondaryDamageMeter.Update();
                _timeSinceSecondaryUpdate.Restart();
            }
        }

        private void OnSecondaryFightEventAdded(FightEvent fightEvent)
        {
            if (fightEvent.LogUnixSeconds < _primaryFightStartUnixSeconds)
                return;

            _latestSecondaryFightEventUnixSeconds = Math.Max(_latestSecondaryFightEventUnixSeconds, fightEvent.LogUnixSeconds);

            // Just the easiest way to keep the secondary damage meter in sync with playback skip aheads on the primary.
            if (_secondaryDamageMeter.IsPlaybackMode)
            {
                int unixSecondsDifference = (int)(_latestPrimaryFightEventUnixSeconds - _latestSecondaryFightEventUnixSeconds);
                if (unixSecondsDifference > 2)
                {
                    _secondaryDamageMeter.SkipAheadPlayback(unixSecondsDifference);
                    _latestSecondaryFightEventUnixSeconds += unixSecondsDifference;
                }
            }

            CheckReflectShield(fightEvent);
            CheckSecondaryNcuWipes(fightEvent);
        }

        // NOTE: keep in sync with CheckNcuWipes in BossModuleViewModelBase.
        // We add nano programs that get cancelled as part of a recast, but then immediately remove
        // them once the recast is proven. So it should be okay--there shouldn't be any UI flicker.
        // One thing we can't easily do is recognize when nanoprograms are terminated by being
        // overwritten by a better nanoprogram. Ideally we wouldn't show those terminations.
        private void CheckSecondaryNcuWipes(FightEvent fightEvent)
        {
            if (fightEvent is SystemEvent systemEvent)
            {
                if (systemEvent.IsNanoDeactivated)
                {
                    _secondaryLastManualNanoProgramDeactivationUnixSeconds = fightEvent.LogUnixSeconds;
                    _secondaryLastManualNanoProgramDeactivationTimestamp = fightEvent.Timestamp;
                }
                else if (systemEvent.IsNanoTerminated)
                {
                    // Manual deactivations can come in a bit before the actual termination.
                    bool isPurposeful = _secondaryLastManualNanoProgramDeactivationUnixSeconds == fightEvent.LogUnixSeconds
                        || _secondaryLastManualNanoProgramDeactivationTimestamp?.AddSeconds(.5) >= fightEvent.Timestamp;
                    _secondaryLastManualNanoProgramDeactivationUnixSeconds = null;
                    _secondaryLastManualNanoProgramDeactivationTimestamp = null;

                    if (!isPurposeful
                        // Just rely on the status bar expiring to signal wipes for these nanos.
                        && !TotalMirrorShield.Nanoline.HasNano(systemEvent.NanoProgram)
                        && !NullitySphere.Nanoline.HasNano(systemEvent.NanoProgram))
                    {
                        if (_secondaryWipedNanoPrograms.TryAdd(systemEvent.NanoProgram, fightEvent.LogUnixSeconds))
                        {
                            _secondaryRecentlyWipedNanoPrograms.TryAdd(systemEvent.NanoProgram, fightEvent.LogUnixSeconds);
                            _timeSinceSecondaryNcuWiped.Restart();
                        }
                    }
                }
                else if (systemEvent.IsFriendlyNanoExecutedOnYou)
                {
                    _secondaryWipedNanoPrograms.TryRemove(systemEvent.NanoProgram, out _);
                    _secondaryRecentlyWipedNanoPrograms.TryRemove(systemEvent.NanoProgram, out _);
                }
            }
            else if (fightEvent is MeCastNano meCastNanoEvent
                && meCastNanoEvent.CastResult == CastResult.Success
                && meCastNanoEvent.NanoProgram != null)
            {
                _secondaryWipedNanoPrograms.TryRemove(meCastNanoEvent.NanoProgram, out _);
                _secondaryRecentlyWipedNanoPrograms.TryRemove(meCastNanoEvent.NanoProgram, out _);
            }
        }

        public override void UpdateView()
        {
            base.UpdateView();

            // Handle the edge case where the primary character is switched with the module already open.
            RaisePropertyChanged(nameof(PrimaryCharacterName));

            if (!HasFightStarted)
                return;

            UpdateSecondaryNcuWipes();
        }

        private void UpdateSecondaryNcuWipes()
        {
            IsSecondaryNcuWipeRecent = !_secondaryRecentlyWipedNanoPrograms.IsEmpty
                && _timeSinceSecondaryNcuWiped.Elapsed.TotalSeconds <= 5;

            if (!IsSecondaryNcuWipeRecent)
            {
                _secondaryRecentlyWipedNanoPrograms.Clear();
            }

            RaisePropertyChanged(nameof(SecondaryWipedNanoPrograms));
            RaisePropertyChanged(nameof(HasSecondaryWipedNanoPrograms));
            RaisePropertyChanged(nameof(SecondaryRecentlyWipedNanoPrograms));
            RaisePropertyChanged(nameof(IsSecondaryNcuWipeRecent));
        }

        public override void Reset()
        {
            base.Reset();

            lock (_secondaryFightLock)
            {
                _primaryFightStartUnixSeconds = long.MaxValue;
                _latestPrimaryFightEventUnixSeconds = 0;
                _latestSecondaryFightEventUnixSeconds = 0;

                if (_secondaryFight != null)
                {
                    _secondaryFight.FightEventAdded -= OnSecondaryFightEventAdded;
                    _secondaryFight = null;
                }

                _timeSinceSecondaryUpdate.Reset();
            }

            ResetSecondaryNcuWipes();
        }

        private void ResetSecondaryNcuWipes()
        {
            _secondaryLastManualNanoProgramDeactivationUnixSeconds = null;
            _secondaryLastManualNanoProgramDeactivationTimestamp = null;
            _timeSinceSecondaryNcuWiped.Reset();
            _secondaryWipedNanoPrograms.Clear();
            _secondaryRecentlyWipedNanoPrograms.Clear();
            IsSecondaryNcuWipeRecent = false;

            RaisePropertyChanged(nameof(SecondaryWipedNanoPrograms));
            RaisePropertyChanged(nameof(HasSecondaryWipedNanoPrograms));
            RaisePropertyChanged(nameof(SecondaryRecentlyWipedNanoPrograms));
            RaisePropertyChanged(nameof(IsSecondaryNcuWipeRecent));
        }

        public override void Dispose()
        {
            Reset();
            _secondaryDamageMeter.Dispose();
        }
    }
}
