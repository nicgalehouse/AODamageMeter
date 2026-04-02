using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using System;
using System.Collections.Generic;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public class TheBeastModuleViewModel : ViewModelBase, IBossModuleViewModel
    {
        private const string TheBeast = "The Beast";
        private const int HitMeHitYouReflectDurationSeconds = 20;
        private const int AddsAreProbablyDeadSeconds = 5;

        private DateTime? _reflectDetectedTimestamp;

        public bool IsReflectActive { get; private set; }
        public Dictionary<string, AddTracker> AddTrackers { get; }

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
            CheckReflectShield(fightEvent);
            CheckAdds(fightEvent);
        }

        private void CheckReflectShield(FightEvent fightEvent)
        {
            if (fightEvent is AttackEvent attackEvent)
            {
                if (!IsReflectActive
                    && attackEvent.DamageType == DamageType.Reflect
                    && (attackEvent.Source.Name == TheBeast || attackEvent is MeHitByMonster))
                {
                    IsReflectActive = true;
                    _reflectDetectedTimestamp = fightEvent.Timestamp;
                }
                else if (IsReflectActive
                    && attackEvent.Target.Name == TheBeast
                    && attackEvent.AttackResult == AttackResult.WeaponHit)
                {
                    IsReflectActive = false;
                    _reflectDetectedTimestamp = null;
                }
            }

            if (IsReflectActive && _reflectDetectedTimestamp.HasValue
                && (fightEvent.Timestamp - _reflectDetectedTimestamp.Value).TotalSeconds >= HitMeHitYouReflectDurationSeconds)
            {
                IsReflectActive = false;
                _reflectDetectedTimestamp = null;
            }
        }

        private void CheckAdds(FightEvent fightEvent)
        {
            if (fightEvent is AttackEvent attackEvent
                && (attackEvent.AttackResult == AttackResult.WeaponHit
                    || attackEvent.AttackResult == AttackResult.NanoHit
                    || attackEvent.AttackResult == AttackResult.Missed)
                // Account for crat charms.
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

        public void UpdateView()
        {
            RaisePropertyChanged(nameof(IsReflectActive));
            RaisePropertyChanged(nameof(AddTrackers));
        }
    }
}
