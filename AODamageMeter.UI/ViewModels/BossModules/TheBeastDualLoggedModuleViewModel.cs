using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Heal;
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

        protected override string StatusBarLabelSuffix => $" - {PrimaryCharacterName}";
        protected string SecondaryStatusBarLabelSuffix => $" - {SecondaryCharacterName}";
        protected override string TauntLabel => $"Taunt";
        private readonly SynchronizedStopwatch _timeSinceSecondaryBossFightStarted = new SynchronizedStopwatch();
        private FixedStatusBarViewModel _secondaryTauntStatusBar;
        private bool _secondaryOwnerHasBeenCastingResonanceBlast;
        private long _secondaryOwnersDamageDone;
        private long _secondaryOwnersHealingDone;
        private long _secondaryOwnersNanoTauntAmount;
        private long _secondaryOwnersNanoDetauntAmount;
        private long SecondaryOwnersTauntAmount => _secondaryOwnersDamageDone + (long)(_secondaryOwnersHealingDone * HealingToTauntFactor)
            + _secondaryOwnersNanoTauntAmount - _secondaryOwnersNanoDetauntAmount;
        private double SecondaryOwnersTauntAmountPM => SecondaryOwnersTauntAmount / _timeSinceSecondaryBossFightStarted.Elapsed.TotalMinutes;
        protected override string DoomOfTheSpiritsLabel => "Doom";

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

            _timeSinceSecondaryBossFightStarted.Start();
            _secondaryTauntStatusBar = RequestFixedStatusBar(
                $"{TauntLabel}{SecondaryStatusBarLabelSuffix}", "0", TauntBarColor, TauntIconPath);
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

            CheckSecondaryNcuWipes(fightEvent);
            CheckSecondaryTaunt(fightEvent);
            CheckSecondaryStatusBars(fightEvent);
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

        // NOTE: keep in sync with CheckTaunt in BossModuleViewModelBase.
        private void CheckSecondaryTaunt(FightEvent fightEvent)
        {
            if (fightEvent is MeCastNano meCastNanoEvent)
            {
                // Common detaunt used by Froob NTs, only one worth worrying about right now.
                if (meCastNanoEvent.NanoProgram == "Resonance Blast")
                {
                    _secondaryOwnerHasBeenCastingResonanceBlast = true;
                }
                // Assume taunts and detaunts will only be used on the boss, and not any adds.
                else if (meCastNanoEvent.CastResult == CastResult.Success
                    && meCastNanoEvent.NanoProgram != null)
                {
                    if (SoldierTaunt.Nanoline.TryGetNano(meCastNanoEvent.NanoProgram, out var nano))
                    {
                        _secondaryOwnersNanoTauntAmount += nano.TauntAmount.Value;
                    }
                    else if (SoldierDetaunt.Nanoline.TryGetNano(meCastNanoEvent.NanoProgram, out nano))
                    {
                        _secondaryOwnersNanoDetauntAmount += nano.DetauntAmount.Value;
                    }
                }
            }

            if (fightEvent is AttackEvent attackEvent
                && attackEvent.Source.IsOwner
                && attackEvent.Target.Name == BossName
                && attackEvent.Amount > 0)
            {
                _secondaryOwnersDamageDone += attackEvent.Amount.Value;

                if (_secondaryOwnerHasBeenCastingResonanceBlast
                    && attackEvent.Amount > 3000
                    && attackEvent.DamageType == DamageType.Radiation)
                {
                    _secondaryOwnersNanoDetauntAmount += 3000;
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
                    _secondaryOwnersHealingDone += (long)(healEvent.Amount.Value * (1 - PercentageOverhealingEstimate));
                }
                else if (healEvent is MeGotHealth)
                {
                    _secondaryOwnersHealingDone += healEvent.Amount.Value;
                }
            }
        }

        // NOTE: keep in sync with CheckStatusBars in TheBeastModuleViewModel and BossModuleViewModelBase.
        private void CheckSecondaryStatusBars(FightEvent fightEvent)
        {
            if (fightEvent is MeCastNano meCastNanoEvent
                && meCastNanoEvent.CastResult == CastResult.Success
                && meCastNanoEvent.NanoProgram != null)
            {
                if (TotalMirrorShield.Nanoline.TryGetNano(meCastNanoEvent.NanoProgram, out var nano)
                    || NullitySphere.Nanoline.TryGetNano(meCastNanoEvent.NanoProgram, out nano))
                {
                    RequestAnimatedStatusBar($"{nano.ShortName}{SecondaryStatusBarLabelSuffix}",
                        nano.DurationSeconds.Value, nano.Color, nano.IconPath);
                }
            }
            else if (fightEvent is SystemEvent systemEvent
                && systemEvent.IsNanoTerminated)
            {
                if (TotalMirrorShield.Nanoline.TryGetNano(systemEvent.NanoProgram, out var nano)
                    || NullitySphere.Nanoline.TryGetNano(systemEvent.NanoProgram, out nano))
                {
                    ExpireStatusBar($"{nano.ShortName}{SecondaryStatusBarLabelSuffix}");
                }
            }

            if (fightEvent is SystemEvent beastSystemEvent && beastSystemEvent.Source?.Name == TheBeast
                && beastSystemEvent.IsHostileNanoExecutedOnYou && beastSystemEvent.NanoProgram == DoomOfTheSpirits)
            {
                RequestAnimatedStatusBar($"{DoomOfTheSpiritsLabel}{SecondaryStatusBarLabelSuffix}",
                    DoomOfTheSpiritsDurationSeconds, DoomOfTheSpiritsBarColor, DoomOfTheSpiritsIconPath);
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
            UpdateSecondaryTaunt();
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

        private void UpdateSecondaryTaunt()
        {
            if (_secondaryTauntStatusBar == null) return;

            _secondaryTauntStatusBar.Value = $"{SecondaryOwnersTauntAmount.Format()} ({SecondaryOwnersTauntAmountPM.Format()})";
            _secondaryTauntStatusBar.RaisePropertyChanged(nameof(FixedStatusBarViewModel.RightText));
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
            ResetSecondaryTaunt();
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

        private void ResetSecondaryTaunt()
        {
            _secondaryOwnerHasBeenCastingResonanceBlast = false;
            _secondaryOwnersDamageDone = 0;
            _secondaryOwnersHealingDone = 0;
            _secondaryOwnersNanoTauntAmount = 0;
            _secondaryOwnersNanoDetauntAmount = 0;
            _timeSinceSecondaryBossFightStarted.Reset();
            _secondaryTauntStatusBar = null;
        }

        public override void Dispose()
        {
            Reset();
            _secondaryDamageMeter.Dispose();
        }
    }
}
