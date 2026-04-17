using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using AODamageMeter.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public class TheBeastModuleViewModel : BossModuleViewModelBase
    {
        protected const string TheBeast = "The Beast";
        private const string TheBeastIconPath = "/Icons/TheBeast.png";

        public override string BossName => TheBeast;
        public override string IconPath => TheBeastIconPath;

        private const int HitMeHitYouReflectDurationSeconds = 20;
        private readonly SynchronizedStopwatch _timeSinceReflectDetected = new SynchronizedStopwatch();
        public bool IsReflectActive { get; private set; }
        public string ReflectCountdown { get; private set; }

        private const int AddsAreProbablyDeadSeconds = 5;
        public Dictionary<string, AddTrackerViewModel> AddTrackers { get; }
        private bool AreAnyAddsActive => AddTrackers.Values.Any(t => t.IsActive);

        private const int CastingDetectionThresholdSeconds = 3;
        private const int CastingFullConfidenceSeconds = 7;
        private readonly SynchronizedStopwatch _timeSinceBeastLastHitOrCast = new SynchronizedStopwatch();
        public bool IsCasting { get; private set; }
        public int CastingDurationSeconds { get; private set; }
        public double CastingOpacity { get; private set; }
        private const int RingOfFireDisplaySeconds = 2;
        private readonly SynchronizedStopwatch _timeSinceRingOfFire = new SynchronizedStopwatch();
        private long _lastBeastNanoHitUnixSeconds;
        private int _beastNanoHitCount;
        public bool IsRingOfFireActive { get; private set; }

        protected const string DoomOfTheSpirits = "Doom Of The Spirits";
        protected virtual string DoomOfTheSpiritsLabel => "Doom Of The Spirits";
        protected const string DoomOfTheSpiritsIconPath = "/Icons/DoomOfTheSpirits.png";
        protected const int DoomOfTheSpiritsDurationSeconds = 18;
        protected const string DoomOfTheSpiritsBarColor = "#7C8289";

        public override bool NeedsIsBossTargetingSomeoneWarning => true;
        public override bool NeedsTauntStatusBar => true;

        public TheBeastModuleViewModel()
            => AddTrackers = new Dictionary<string, AddTrackerViewModel>
            {
                ["Corrupted Xan-Kuir"] = new AddTrackerViewModel("Corrupted Xan-Kuir"),
                ["Corrupted Xan-Len"] = new AddTrackerViewModel("Corrupted Xan-Len"),
                ["Corrupted Hiisi Berserker"] = new AddTrackerViewModel("Corrupted Hiisi Berserker"),
                ["Corrupted Xan-Cur"] = new AddTrackerViewModel("Corrupted Xan-Cur"),
            };

        protected override void OnFightStarted(FightEvent fightEvent)
        {
            base.OnFightStarted(fightEvent);
            _timeSinceBeastLastHitOrCast.Start();
        }

        public override void OnFightEventAdded(FightEvent fightEvent)
        {
            base.OnFightEventAdded(fightEvent);

            if (HasFightStarted)
            {
                CheckReflectShield(fightEvent);
                CheckAdds(fightEvent);
                CheckCasting(fightEvent);
                CheckRingOfFire(fightEvent);
            }
        }

        protected void CheckReflectShield(FightEvent fightEvent)
        {
            if (fightEvent is AttackEvent attackEvent)
            {
                if (!IsReflectActive
                    && attackEvent.DamageType == DamageType.Reflect
                    // The player themselves only sees "Someone's reflect shield hit you"--assume it's from The Beast.
                    && (attackEvent.Source.Name == TheBeast || attackEvent is MeHitByMonster))
                {
                    IsReflectActive = true;
                    ReflectCountdown = $"{HitMeHitYouReflectDurationSeconds}s";
                    _timeSinceReflectDetected.Restart();
                }
                else if (IsReflectActive
                    && attackEvent.Target.Name == TheBeast
                    && attackEvent.AttackResult == AttackResult.WeaponHit)
                {
                    IsReflectActive = false;
                    ReflectCountdown = null;
                    _timeSinceReflectDetected.Reset();
                }
            }
        }

        protected void CheckAdds(FightEvent fightEvent)
        {
            if (fightEvent is AttackEvent attackEvent
                && (attackEvent.AttackResult == AttackResult.WeaponHit
                    || attackEvent.AttackResult == AttackResult.NanoHit
                    || attackEvent.AttackResult == AttackResult.Missed)
                // Don't let crat charms count as adds.
                && attackEvent.Source.Name != TheBeast
                && attackEvent.Target.Name != TheBeast)
            {
                if (AddTrackers.TryGetValue(attackEvent.Source.Name, out var sourceAddTracker))
                {
                    sourceAddTracker.Activate(fightEvent.Timestamp);
                }

                if (AddTrackers.TryGetValue(attackEvent.Target.Name, out var targetAddTracker))
                {
                    targetAddTracker.Activate(fightEvent.Timestamp);
                }
            }

            foreach (var addTracker in AddTrackers.Values)
            {
                if (addTracker.IsActive && addTracker.DetectedTimestamp.HasValue
                    && (fightEvent.Timestamp - addTracker.DetectedTimestamp.Value).TotalSeconds >= AddsAreProbablyDeadSeconds)
                {
                    addTracker.Deactivate();
                }
            }
        }

        protected void CheckCasting(FightEvent fightEvent)
        {
            if (fightEvent is AttackEvent attackEvent
                && (attackEvent.Source.Name == TheBeast
                    && (attackEvent.AttackResult == AttackResult.WeaponHit
                        || attackEvent.AttackResult == AttackResult.NanoHit
                        || attackEvent.AttackResult == AttackResult.Missed)
                    // We don't know the source or target of absorbed hits--when adds are up, ignore them as a signal.
                    || attackEvent.AttackResult == AttackResult.Absorbed && !AreAnyAddsActive)
                || fightEvent is SystemEvent systemEvent
                    && (systemEvent.Source?.Name == TheBeast
                        && (systemEvent.IsHostileNanoExecutedOnYou || systemEvent.IsHostileNanoCounteredByYou)
                        // We don't know the source of blocked hits--when adds are up, ignore them as a signal.
                        || systemEvent.IsYouBlockedRegular && !AreAnyAddsActive))
            {
                IsCasting = false;
                CastingDurationSeconds = 0;
                CastingOpacity = 0;
                _timeSinceBeastLastHitOrCast.Restart();
            }
        }

        private void CheckRingOfFire(FightEvent fightEvent)
        {
            if (fightEvent is AttackEvent attackEvent
                && attackEvent.Source.Name == TheBeast
                && attackEvent.AttackResult == AttackResult.NanoHit)
            {
                if (fightEvent.LogUnixSeconds == _lastBeastNanoHitUnixSeconds)
                {
                    _beastNanoHitCount++;
                }
                else
                {
                    _lastBeastNanoHitUnixSeconds = fightEvent.LogUnixSeconds;
                    _beastNanoHitCount = 1;
                }

                if (_beastNanoHitCount >= 2)
                {
                    IsRingOfFireActive = true;
                    _timeSinceRingOfFire.Restart();
                }
            }
        }

        // NOTE: keep in sync with CheckSecondaryStatusBars in TheBeastDualLoggedModuleViewModel.
        protected override void CheckStatusBars(FightEvent fightEvent)
        {
            base.CheckStatusBars(fightEvent);

            if (fightEvent is SystemEvent systemEvent && systemEvent.Source?.Name == TheBeast
                && systemEvent.IsHostileNanoExecutedOnYou && systemEvent.NanoProgram == DoomOfTheSpirits)
            {
                RequestAnimatedStatusBar($"{DoomOfTheSpiritsLabel}{StatusBarLabelSuffix}",
                    DoomOfTheSpiritsDurationSeconds, DoomOfTheSpiritsBarColor, DoomOfTheSpiritsIconPath);
            }
        }

        public override void UpdateView()
        {
            base.UpdateView();

            if (!HasFightStarted)
                return;

            UpdateReflects();
            UpdateAdds();
            UpdateCasting();
            UpdateRingOfFire();
        }

        private void UpdateReflects()
        {
            if (IsReflectActive)
            {
                int remainingReflectDuration = HitMeHitYouReflectDurationSeconds
                    - (int)_timeSinceReflectDetected.Elapsed.TotalSeconds;

                if (remainingReflectDuration <= 0)
                {
                    IsReflectActive = false;
                    ReflectCountdown = null;
                    _timeSinceReflectDetected.Reset();
                }
                else
                {
                    ReflectCountdown = $"<{remainingReflectDuration}s";
                }
            }

            RaisePropertyChanged(nameof(IsReflectActive));
            RaisePropertyChanged(nameof(ReflectCountdown));
        }

        private void UpdateAdds()
            => RaisePropertyChanged(nameof(AddTrackers));

        private void UpdateCasting()
        {
            double secondsSinceLastHit = _timeSinceBeastLastHitOrCast.Elapsed.TotalSeconds;

            if (secondsSinceLastHit >= CastingDetectionThresholdSeconds)
            {
                IsCasting = true;
                CastingDurationSeconds = (int)secondsSinceLastHit;
                double progress = Math.Min(1.0, (secondsSinceLastHit - CastingDetectionThresholdSeconds)
                    / (CastingFullConfidenceSeconds - CastingDetectionThresholdSeconds));
                CastingOpacity = 0.1 + (0.9 * progress);
            }

            RaisePropertyChanged(nameof(IsCasting));
            RaisePropertyChanged(nameof(CastingDurationSeconds));
            RaisePropertyChanged(nameof(CastingOpacity));
        }

        private void UpdateRingOfFire()
        {
            if (IsRingOfFireActive
                && _timeSinceRingOfFire.Elapsed.TotalSeconds >= RingOfFireDisplaySeconds)
            {
                _timeSinceRingOfFire.Reset();
                IsRingOfFireActive = false;
            }

            RaisePropertyChanged(nameof(IsRingOfFireActive));
        }

        public override void Reset()
        {
            base.Reset();

            ResetReflects();
            ResetAdds();
            ResetCasting();
            ResetRingOfFire();
        }

        private void ResetReflects()
        {
            _timeSinceReflectDetected.Reset();
            IsReflectActive = false;
            ReflectCountdown = null;

            RaisePropertyChanged(nameof(IsReflectActive));
            RaisePropertyChanged(nameof(ReflectCountdown));
        }

        private void ResetAdds()
        {
            foreach (var tracker in AddTrackers.Values)
            {
                tracker.Deactivate();
            }

            RaisePropertyChanged(nameof(AddTrackers));
        }

        private void ResetCasting()
        {
            _timeSinceBeastLastHitOrCast.Reset();
            IsCasting = false;
            CastingDurationSeconds = 0;
            CastingOpacity = 0;

            RaisePropertyChanged(nameof(IsCasting));
            RaisePropertyChanged(nameof(CastingDurationSeconds));
            RaisePropertyChanged(nameof(CastingOpacity));
        }

        private void ResetRingOfFire()
        {
            _timeSinceRingOfFire.Reset();
            _lastBeastNanoHitUnixSeconds = 0;
            _beastNanoHitCount = 0;
            IsRingOfFireActive = false;

            RaisePropertyChanged(nameof(IsRingOfFireActive));
        }
    }
}
