using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public class TheBeastModuleViewModel : ViewModelBase, IBossModuleViewModel
    {
        private const string TheBeast = "The Beast";

        private DateTime? _lastManualNanoProgramDeactivationTimestamp;
        private readonly List<string> _wipedNanoPrograms = new List<string>();
        public List<string> WipedNanoPrograms => new List<string>(_wipedNanoPrograms);
        public bool HasWipedNanoPrograms => _wipedNanoPrograms.Count > 0;

        private const int HitMeHitYouReflectDurationSeconds = 20;
        private readonly Stopwatch _timeSinceReflectDetected = new Stopwatch();
        public bool IsReflectActive { get; private set; }

        private const int AddsAreProbablyDeadSeconds = 5;
        public Dictionary<string, AddTracker> AddTrackers { get; }

        private const int CastingDetectionThresholdSeconds = 3;
        private const int CastingFullConfidenceSeconds = 7;
        private readonly Stopwatch _timeSinceBeastLastHitSomeone = new Stopwatch();
        public bool IsCasting { get; private set; }
        public int CastingDurationSeconds { get; private set; }
        public double CastingOpacity { get; private set; }

        public TheBeastModuleViewModel()
            => AddTrackers = new Dictionary<string, AddTracker>
            {
                ["Corrupted Xan-Kuir"] = new AddTracker("Corrupted Xan-Kuir"),
                ["Corrupted Xan-Len"] = new AddTracker("Corrupted Xan-Len"),
                ["Corrupted Hiisi Berserker"] = new AddTracker("Corrupted Hiisi Berserker"),
                ["Corrupted Xan-Cur"] = new AddTracker("Corrupted Xan-Cur"),
            };

        public void OnFightEventAdded(FightEvent fightEvent)
        {
            // Catch the edge case where the fight begins with The Beast casting something.
            if (!_timeSinceBeastLastHitSomeone.IsRunning
                && fightEvent is AttackEvent attackEvent
                && (attackEvent.Source.Name == TheBeast || attackEvent.Target.Name == TheBeast))
            {
                _timeSinceBeastLastHitSomeone.Start();
            }

            CheckNcuWipe(fightEvent);
            CheckReflectShield(fightEvent);
            CheckAdds(fightEvent);
            CheckCasting(fightEvent);
        }

        // We add nano programs that get cancelled as part of a recast, but then immediately remove
        // them, once the recast is proven. So it should be okay. There shouldn't be any UI flicker.
        private void CheckNcuWipe(FightEvent fightEvent)
        {
            if (fightEvent is SystemEvent systemEvent)
            {
                if (systemEvent.IsNanoDeactivated)
                {
                    _lastManualNanoProgramDeactivationTimestamp = fightEvent.Timestamp;
                }
                else if (systemEvent.IsNanoTerminated)
                {
                    bool isPurposeful = _lastManualNanoProgramDeactivationTimestamp == fightEvent.Timestamp;
                    _lastManualNanoProgramDeactivationTimestamp = null;

                    if (!isPurposeful && !_wipedNanoPrograms.Contains(systemEvent.NanoProgram))
                    {
                        _wipedNanoPrograms.Add(systemEvent.NanoProgram);
                    }
                }
                else if (systemEvent.IsNanoExecutedByOther)
                {
                    _wipedNanoPrograms.Remove(systemEvent.NanoProgram);
                }
            }
            else if (fightEvent is MeCastNano castEvent
                && castEvent.CastResult == CastResult.Success
                && castEvent.NanoProgram != null)
            {
                _wipedNanoPrograms.Remove(castEvent.NanoProgram);
            }
        }

        private void CheckReflectShield(FightEvent fightEvent)
        {
            if (fightEvent is AttackEvent attackEvent)
            {
                if (!IsReflectActive
                    && attackEvent.DamageType == DamageType.Reflect
                    // The player themselves only sees "Someone's reflect shield hit you"--assume it's from The Beast.
                    && (attackEvent.Source.Name == TheBeast || attackEvent is MeHitByMonster))
                {
                    IsReflectActive = true;
                    _timeSinceReflectDetected.Restart();
                }
                else if (IsReflectActive
                    && attackEvent.Target.Name == TheBeast
                    && attackEvent.AttackResult == AttackResult.WeaponHit)
                {
                    IsReflectActive = false;
                    _timeSinceReflectDetected.Reset();
                }
            }
        }

        private void CheckAdds(FightEvent fightEvent)
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

        private bool AreAnyAddsActive
            => AddTrackers.Values.Any(t => t.IsActive);

        private void CheckCasting(FightEvent fightEvent)
        {
            if (fightEvent is AttackEvent attackEvent
                && (attackEvent.Source.Name == TheBeast && attackEvent.AttackResult == AttackResult.WeaponHit
                    // We don't know the source or target of absorbed hits--when adds are up, ignore them as a signal.
                    || attackEvent.AttackResult == AttackResult.Absorbed && !AreAnyAddsActive))
            {
                _timeSinceBeastLastHitSomeone.Restart();
                IsCasting = false;
                CastingDurationSeconds = 0;
                CastingOpacity = 0;
            }
        }

        public void UpdateView()
        {
            if (IsReflectActive
                && _timeSinceReflectDetected.Elapsed.TotalSeconds >= HitMeHitYouReflectDurationSeconds)
            {
                IsReflectActive = false;
                _timeSinceReflectDetected.Reset();
            }

            if (_timeSinceBeastLastHitSomeone.IsRunning)
            {
                double secondsSinceLastHit = _timeSinceBeastLastHitSomeone.Elapsed.TotalSeconds;

                if (secondsSinceLastHit >= CastingDetectionThresholdSeconds)
                {
                    IsCasting = true;
                    CastingDurationSeconds = (int)secondsSinceLastHit;
                    double progress = Math.Min(1.0, (secondsSinceLastHit - CastingDetectionThresholdSeconds)
                        / (CastingFullConfidenceSeconds - CastingDetectionThresholdSeconds));
                    CastingOpacity = 0.1 + (0.9 * progress);
                }
            }

            RaisePropertyChanged(nameof(WipedNanoPrograms));
            RaisePropertyChanged(nameof(HasWipedNanoPrograms));
            RaisePropertyChanged(nameof(IsReflectActive));
            RaisePropertyChanged(nameof(AddTrackers));
            RaisePropertyChanged(nameof(IsCasting));
            RaisePropertyChanged(nameof(CastingDurationSeconds));
            RaisePropertyChanged(nameof(CastingOpacity));
        }

        public void Reset()
        {
            _lastManualNanoProgramDeactivationTimestamp = null;
            _wipedNanoPrograms.Clear();

            _timeSinceReflectDetected.Reset();
            IsReflectActive = false;

            foreach (var tracker in AddTrackers.Values)
            {
                tracker.Deactivate();
            }

            _timeSinceBeastLastHitSomeone.Reset();
            IsCasting = false;
            CastingDurationSeconds = 0;
            CastingOpacity = 0;
        }
    }
}
