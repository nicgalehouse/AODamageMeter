using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using System;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public class TheBeastModuleViewModel : ViewModelBase, IBossModuleViewModel
    {
        private const string BossName = "The Beast";
        private const int HitMeHitYouReflectDurationSeconds = 20;

        private DateTime? _reflectDetectedTimestamp;

        public bool IsReflectActive { get; private set; }

        public void OnFightEventAdded(FightEvent fightEvent)
            => CheckReflectShield(fightEvent);

        private void CheckReflectShield(FightEvent fightEvent)
        {
            if (fightEvent is AttackEvent attackEvent)
            {
                if (!IsReflectActive
                    && attackEvent.DamageType == DamageType.Reflect
                    && (attackEvent.Source.Name == BossName || attackEvent is MeHitByMonster))
                {
                    IsReflectActive = true;
                    _reflectDetectedTimestamp = fightEvent.Timestamp;
                }
                else if (IsReflectActive
                    && attackEvent.Target.Name == BossName
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

        public void UpdateView()
            => RaisePropertyChanged(nameof(IsReflectActive));
    }
}
