using System.Diagnostics;

namespace AODamageMeter.UI.ViewModels.BossModules
{
    public class TheBeastDualLoggedModuleViewModel : TheBeastModuleViewModel
    {
        private readonly object _secondaryFightLock = new object();
        private readonly DamageMeter _secondaryDamageMeter;
        private readonly Stopwatch _timeSinceSecondaryUpdate = new Stopwatch();
        private long _primaryFightStartUnixSeconds;
        private Fight _secondaryFight;

        public string SecondaryCharacterName { get; }

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

            // Stub -- secondary character event handling will be built out here.
        }

        public override void Reset()
        {
            base.Reset();

            lock (_secondaryFightLock)
            {
                _primaryFightStartUnixSeconds = 0;

                if (_secondaryFight != null)
                {
                    _secondaryFight.FightEventAdded -= OnSecondaryFightEventAdded;
                    _secondaryFight = null;
                }

                _timeSinceSecondaryUpdate.Reset();
            }
        }

        public override void Dispose()
        {
            Reset();
            _secondaryDamageMeter.Dispose();
        }
    }
}
