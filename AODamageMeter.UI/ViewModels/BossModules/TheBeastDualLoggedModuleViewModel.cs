namespace AODamageMeter.UI.ViewModels.BossModules
{
    public class TheBeastDualLoggedModuleViewModel : TheBeastModuleViewModel
    {
        private volatile bool _needsSecondaryFightInitialization;
        private long _fightStartUnixSeconds;
        private readonly DamageMeter _secondaryDamageMeter;
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
            _fightStartUnixSeconds = fightEvent.LogUnixSeconds;
            _needsSecondaryFightInitialization = true;
        }

        private void OnSecondaryFightEventAdded(FightEvent fightEvent)
        {
            if (fightEvent.LogUnixSeconds < _fightStartUnixSeconds)
                return;

            // Stub -- secondary character event handling will be built out here.
        }

        public override void UpdateView()
        {
            base.UpdateView();

            if (!HasFightStarted)
            {
                _needsSecondaryFightInitialization = false;
                return;
            }

            if (_needsSecondaryFightInitialization)
            {
                if (_secondaryFight != null)
                {
                    _secondaryFight.FightEventAdded -= OnSecondaryFightEventAdded;
                }

                _secondaryDamageMeter.InitializeNewFight();
                _secondaryFight = _secondaryDamageMeter.CurrentFight;
                _secondaryFight.FightEventAdded += OnSecondaryFightEventAdded;

                _needsSecondaryFightInitialization = false;
            }

            _secondaryDamageMeter.Update();
        }

        public override void Reset()
        {
            base.Reset();

            _needsSecondaryFightInitialization = false;
            _fightStartUnixSeconds = 0;

            if (_secondaryFight != null)
            {
                _secondaryFight.FightEventAdded -= OnSecondaryFightEventAdded;
                _secondaryFight = null;
            }
        }

        public override void Dispose()
            => _secondaryDamageMeter.Dispose();
    }
}
