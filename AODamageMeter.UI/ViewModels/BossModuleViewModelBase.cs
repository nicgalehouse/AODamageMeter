using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using AODamageMeter.FightEvents.Heal;
using AODamageMeter.Nanolines;
using AODamageMeter.UI.Helpers;
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

        private readonly ConcurrentQueue<(StatusBarViewModelBase pendingStatusBar, string pendingExpiredStatusBarKey)>
            _pendingStatusBarUpdates = new ConcurrentQueue<(StatusBarViewModelBase, string)>();
        public ObservableCollection<StatusBarViewModelBase> StatusBars { get; } = new ObservableCollection<StatusBarViewModelBase>();
        public bool HasStatusBars => StatusBars.Count > 0;

        public virtual bool NeedsIsBossTargetingYouWarning => false;
        public bool IsBossTargetingYou { get; private set; }
        public string IsBossTargetingYouText => $"{BossName} is on you!";

        public virtual bool NeedsTauntStatusBar => false;
        private const string TauntLabel = "Taunt Estimate";
        private const string TauntIconPath = "/Icons/Taunt.png";
        private const string TauntBarColor = "#D4A030";
        private const double HealingToTauntFactor = 0.5;
        private const double PercentageOverhealingEstimate = 0.25;
        private const int MaxHealingFromTeamEnhancedDeathlessBlessing = 347;
        private readonly SynchronizedStopwatch _timeSinceBossFightStarted = new SynchronizedStopwatch();
        private FixedStatusBarViewModel _tauntStatusBar;
        private bool _ownerHasBeenCastingResonanceBlast;
        private long _ownersDamageDone;
        private long _ownersHealingDone;
        private long _ownersNanoTauntAmount;
        private long _ownersNanoDetauntAmount;
        private long OwnersTauntAmount => _ownersDamageDone + (long)(_ownersHealingDone * HealingToTauntFactor)
            + _ownersNanoTauntAmount - _ownersNanoDetauntAmount;
        private double OwnersTauntAmountPM => OwnersTauntAmount / _timeSinceBossFightStarted.Elapsed.TotalMinutes;

        protected virtual void OnFightStarted()
        {
            if (NeedsTauntStatusBar)
            {
                _timeSinceBossFightStarted.Start();
                _tauntStatusBar = RequestFixedStatusBar(TauntLabel, "0", TauntBarColor, TauntIconPath);
            }
        }

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

                if (NeedsIsBossTargetingYouWarning)
                {
                    CheckIsBossTargetingYou(fightEvent);
                }

                if (NeedsTauntStatusBar)
                {
                    CheckTaunt(fightEvent);
                }
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

                    if (!isPurposeful
                        // Just rely on the status bar expiring to signal wipes for these nanos.
                        && !TotalMirrorShield.Nanoline.HasNano(systemEvent.NanoProgram)
                        && !NullitySphere.Nanoline.HasNano(systemEvent.NanoProgram))
                    {
                        _wipedNanoPrograms.TryAdd(systemEvent.NanoProgram, fightEvent.LogUnixSeconds);
                    }
                }
                else if (systemEvent.IsFriendlyNanoExecutedOnYou)
                {
                    _wipedNanoPrograms.TryRemove(systemEvent.NanoProgram, out _);
                }
            }
            else if (fightEvent is MeCastNano meCastNanoEvent
                && meCastNanoEvent.CastResult == CastResult.Success
                && meCastNanoEvent.NanoProgram != null)
            {
                _wipedNanoPrograms.TryRemove(meCastNanoEvent.NanoProgram, out _);
            }
        }

        protected virtual void CheckStatusBars(FightEvent fightEvent)
        {
            if (fightEvent is MeCastNano meCastNanoEvent
                && meCastNanoEvent.CastResult == CastResult.Success
                && meCastNanoEvent.NanoProgram != null)
            {
                if (TotalMirrorShield.Nanoline.TryGetNano(meCastNanoEvent.NanoProgram, out var nano)
                    || NullitySphere.Nanoline.TryGetNano(meCastNanoEvent.NanoProgram, out nano))
                {
                    RequestAnimatedStatusBar(nano.ShortName, nano.DurationSeconds.Value, nano.Color, nano.IconPath);
                }
            }
            else if (fightEvent is SystemEvent systemEvent
                && systemEvent.IsNanoTerminated)
            {
                if (TotalMirrorShield.Nanoline.TryGetNano(systemEvent.NanoProgram, out var nano)
                    || NullitySphere.Nanoline.TryGetNano(systemEvent.NanoProgram, out nano))
                {
                    ExpireStatusBar(nano.ShortName);
                }
            }
        }

        private void CheckIsBossTargetingYou(FightEvent fightEvent)
        {
            if (fightEvent is SystemEvent systemEvent
                && systemEvent.IsAttackedByOther && systemEvent.Source?.Name == BossName)
            {
                IsBossTargetingYou = true;
            }
            else if (fightEvent is AttackEvent attackEvent
                && attackEvent.Source.Name == BossName && attackEvent.Target.IsOwner
                && (attackEvent.AttackResult == AttackResult.WeaponHit || attackEvent.AttackResult == AttackResult.Missed))
            {
                IsBossTargetingYou = true;
            }
            else if (fightEvent is OtherHitByOther otherHitByOther
                && otherHitByOther.Source.Name == BossName && otherHitByOther.AttackResult == AttackResult.WeaponHit)
            {
                IsBossTargetingYou = false;
            }
        }

        private void CheckTaunt(FightEvent fightEvent)
        {
            if (fightEvent is MeCastNano meCastNanoEvent)
            {
                // Common detaunt used by Froob NTs, only one worth worrying about right now.
                if (meCastNanoEvent.NanoProgram == "Resonance Blast")
                {
                    _ownerHasBeenCastingResonanceBlast = true;
                }
                // Assume taunts and detaunts will only be used on the boss, and not any adds.
                else if (meCastNanoEvent.CastResult == CastResult.Success
                    && meCastNanoEvent.NanoProgram != null)
                {
                    if (SoldierTaunt.Nanoline.TryGetNano(meCastNanoEvent.NanoProgram, out var nano))
                    {
                        _ownersNanoTauntAmount += nano.TauntAmount.Value;
                    }
                    else if (SoldierDetaunt.Nanoline.TryGetNano(meCastNanoEvent.NanoProgram, out nano))
                    {
                        _ownersNanoDetauntAmount += nano.DetauntAmount.Value;
                    }
                }
            }

            if (fightEvent is AttackEvent attackEvent
                && attackEvent.Source.IsOwner
                && attackEvent.Target.Name == BossName
                && attackEvent.Amount > 0)
            {
                _ownersDamageDone += attackEvent.Amount.Value;

                if (_ownerHasBeenCastingResonanceBlast
                    && attackEvent.Amount > 3000
                    && attackEvent.DamageType == DamageType.Radiation)
                {
                    _ownersNanoDetauntAmount += 3000;
                }
            }
            else if (fightEvent is HealEvent healEvent
                && healEvent.Source?.IsOwner == true
                && healEvent.HealType != HealType.Nano
                // We believe HoTs don't generate taunt. This excludes self-healing from heal delta but I'm
                // not sure if that generates taunt or not, and that usually won't be a significant factor.
                && healEvent.Amount > MaxHealingFromTeamEnhancedDeathlessBlessing)
            {
                // We know the owner is the one healing--if they're giving health to someone else we can only see
                // potential healing (oof), and if they're healing themselves, we can only see the realized healing.
                // I'm guessing that only realized healing contributes to taunt.
                if (healEvent is YouGaveHealth)
                {
                    _ownersHealingDone += (long)(healEvent.Amount.Value * (1 - PercentageOverhealingEstimate));
                }
                else if (healEvent is MeGotHealth)
                {
                    _ownersHealingDone += healEvent.Amount.Value;
                }
            }
        }

        protected AnimatedStatusBarViewModel RequestAnimatedStatusBar(
            string label, double totalSeconds, string color, string iconPath)
        {
            var animatedStatusBarViewModel = new AnimatedStatusBarViewModel(label, totalSeconds, color, iconPath);
            _pendingStatusBarUpdates.Enqueue((animatedStatusBarViewModel, null));

            return animatedStatusBarViewModel;
        }

        protected FixedStatusBarViewModel RequestFixedStatusBar(
            string label, string value, string color, string iconPath)
        {
            var fixedStatusBarViewModel = new FixedStatusBarViewModel(label, value, color, iconPath);
            _pendingStatusBarUpdates.Enqueue((fixedStatusBarViewModel, null));

            return fixedStatusBarViewModel;
        }

        protected void ExpireStatusBar(string key)
            => _pendingStatusBarUpdates.Enqueue((null, key));

        public virtual void UpdateView()
        {
            if (!HasFightStarted)
                return;

            UpdateNcuWipes();
            UpdateStatusBars();
            UpdateIsBossTargetingYou();
            UpdateTaunt();
        }

        private void UpdateNcuWipes()
        {
            RaisePropertyChanged(nameof(WipedNanoPrograms));
            RaisePropertyChanged(nameof(HasWipedNanoPrograms));
        }

        private void UpdateStatusBars()
        {
            bool statusBarsCollectionChanged = false;

            while (_pendingStatusBarUpdates.TryDequeue(out var update))
            {
                if (update.pendingStatusBar != null)
                {
                    var existingStatusBar = StatusBars
                        .FirstOrDefault(b => b.Key == update.pendingStatusBar.Key);

                    if (existingStatusBar != null)
                    {
                        StatusBars[StatusBars.IndexOf(existingStatusBar)] = update.pendingStatusBar;
                    }
                    else
                    {
                        StatusBars.Add(update.pendingStatusBar);
                        statusBarsCollectionChanged = true;
                    }
                }
                else
                {
                    var existingStatusBar = StatusBars
                        .FirstOrDefault(b => b.Key == update.pendingExpiredStatusBarKey);

                    if (existingStatusBar != null)
                    {
                        StatusBars.Remove(existingStatusBar);
                        statusBarsCollectionChanged = true;
                    }
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

        private void UpdateIsBossTargetingYou()
        {
            if (NeedsIsBossTargetingYouWarning)
            {
                RaisePropertyChanged(nameof(IsBossTargetingYou));
            }
        }

        private void UpdateTaunt()
        {
            if (!NeedsTauntStatusBar || _tauntStatusBar == null) return;

            _tauntStatusBar.Value = $"{OwnersTauntAmount.Format()} ({OwnersTauntAmountPM.Format()})";
            _tauntStatusBar.RaisePropertyChanged(nameof(FixedStatusBarViewModel.RightText));
        }

        public virtual void Reset()
        {
            HasFightStarted = false;

            ResetNcuWipes();
            ResetStatusBars();
            ResetIsBossTargetingYou();
            ResetTaunt();
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
            while (_pendingStatusBarUpdates.TryDequeue(out _)) { }
            StatusBars.Clear();

            RaisePropertyChanged(nameof(HasStatusBars));
        }

        private void ResetIsBossTargetingYou()
        {
            IsBossTargetingYou = false;

            RaisePropertyChanged(nameof(IsBossTargetingYou));
        }

        private void ResetTaunt()
        {
            _ownerHasBeenCastingResonanceBlast = false;
            _ownersDamageDone = 0;
            _ownersHealingDone = 0;
            _ownersNanoTauntAmount = 0;
            _ownersNanoDetauntAmount = 0;
            _timeSinceBossFightStarted.Reset();
            _tauntStatusBar = null;
        }
    }
}
