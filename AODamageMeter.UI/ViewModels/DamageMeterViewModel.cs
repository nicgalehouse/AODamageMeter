using AODamageMeter.UI.Properties;
using AODamageMeter.UI.ViewModels.Rows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AODamageMeter.UI.ViewModels
{
    public sealed class DamageMeterViewModel : ViewModelBase
    {
        private string _characterName;
        private string _logFilePath;
        private IProgress<object> _rowUpdater;
        private CancellationTokenSource _damageMeterUpdaterCTS;
        private Task _damageMeterUpdater;
        private bool _isDamageMeterUpdaterStarted;

        public DamageMeterViewModel()
        {
            ResetDamageMeterCommand = new RelayCommand(ExecuteResetDamageMeterCommand);
            ToggleIsPausedCommand = new RelayCommand(ExecuteToggleIsPausedCommand);
            _rowUpdater = new Progress<object>(_ => SetAndUpdateRows());
            TryInitializingDamageMeter(Settings.Default.SelectedCharacterName, Settings.Default.SelectedLogFilePath);
        }

        public DamageMeter DamageMeter { get; private set; }

        private readonly ObservableCollection<MainRowBase> _viewingModeRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<FightCharacter, MainRowBase> _damageDoneRowMap = new Dictionary<FightCharacter, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _damageDoneRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowBase>> _damageDoneInfoRowMapMap = new Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowBase>>();
        private readonly Dictionary<FightCharacter, ObservableCollection<MainRowBase>> _damageDoneInfoRowsMap = new Dictionary<FightCharacter, ObservableCollection<MainRowBase>>();
        private readonly Dictionary<FightCharacter, MainRowBase> _damageTakenRowMap = new Dictionary<FightCharacter, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _damageTakenRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowBase>> _damageTakenInfoRowMapMap = new Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowBase>>();
        private readonly Dictionary<FightCharacter, ObservableCollection<MainRowBase>> _damageTakenInfoRowsMap = new Dictionary<FightCharacter, ObservableCollection<MainRowBase>>();
        private readonly Dictionary<HealingInfo, MainRowBase> _ownersHealingDoneRowMap = new Dictionary<HealingInfo, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _ownersHealingDoneRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<HealingInfo, MainRowBase> _ownersHealingTakenRowMap = new Dictionary<HealingInfo, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _ownersHealingTakenRows = new ObservableCollection<MainRowBase>();
        private readonly Dictionary<CastInfo, MainRowBase> _ownersCastsRowMap = new Dictionary<CastInfo, MainRowBase>();
        private readonly ObservableCollection<MainRowBase> _ownersCastsRows = new ObservableCollection<MainRowBase>();
        private ObservableCollection<MainRowBase> _rows;
        public ObservableCollection<MainRowBase> Rows
        {
            get => _rows;
            set => Set(ref _rows, value);
        }

        private ViewingMode _selectedViewingMode = ViewingMode.DamageDone;
        public ViewingMode SelectedViewingMode
        {
            get => _selectedViewingMode;
            private set => Set(ref _selectedViewingMode, value);
        }

        // Using Character rather than FightCharacter so the view can exist independently of damage meter resets.
        private Character _selectedCharacter;
        public Character SelectedCharacter
        {
            get => _selectedCharacter;
            set => Set(ref _selectedCharacter, value);
        }

        public bool TryInitializingDamageMeter(string characterName, string logFilePath)
        {
            // No reason to reinitialize if same name/path AND we succeeded before (i.e., _damageMeter is not null).
            if (_characterName == characterName && _logFilePath == logFilePath && DamageMeter != null)
                return true;

            _characterName = characterName;
            _logFilePath = logFilePath;

            DisposeDamageMeter();
            ClearRows();

            if (string.IsNullOrWhiteSpace(logFilePath)) return false;
            if (!File.Exists(logFilePath))
            {
                try { File.Create(logFilePath); }
                catch { return false; }
            }

            DamageMeter = Character.FitsPlayerNamingRequirements(characterName)
                ? new DamageMeter(characterName, logFilePath)
                : new DamageMeter(logFilePath);
            DamageMeter.IsPaused = IsPaused;
#if DEBUG
            DamageMeter.InitializeNewFight(skipToEndOfLog: false);
#else
            DamageMeter.InitializeNewFight();
#endif
            StartDamageMeterUpdater();

            if (SelectedViewingMode == ViewingMode.OwnersHealingDone
                || SelectedViewingMode == ViewingMode.OwnersHealingTaken
                || SelectedViewingMode == ViewingMode.OwnersCasts)
            {
                SelectedCharacter = DamageMeter.Owner;
            }

            return true;
        }

        private void StartDamageMeterUpdater()
        {
            if (_isDamageMeterUpdaterStarted) return;

            _damageMeterUpdaterCTS = new CancellationTokenSource();
            _damageMeterUpdater = Task.Factory.StartNew(() =>
            {
                do
                {
                    lock (DamageMeter)
                    {
                        DamageMeter.Update().Wait();
                    }
                    if (_damageMeterUpdaterCTS.IsCancellationRequested) return;
                    _rowUpdater.Report(null);
                } while (!_damageMeterUpdaterCTS.Token.WaitHandle.WaitOne(300));
            }, _damageMeterUpdaterCTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _isDamageMeterUpdaterStarted = true;
        }

        public bool TryProgressingView(MainRowBase mainRowViewModelBase)
        {
            switch (SelectedViewingMode)
            {
                case ViewingMode.ViewingModes:
                    var viewingMode = ((ViewingModeMainRowBase)mainRowViewModelBase).ViewingMode;
                    if (viewingMode == ViewingMode.OwnersXP) return false;

                    SelectedViewingMode = viewingMode;
                    if (SelectedViewingMode == ViewingMode.OwnersHealingDone
                        || SelectedViewingMode == ViewingMode.OwnersHealingTaken
                        || SelectedViewingMode == ViewingMode.OwnersCasts)
                    {
                        SelectedCharacter = DamageMeter.Owner;
                    }
                    break;
                case ViewingMode.DamageDone:
                    SelectedViewingMode = ViewingMode.DamageDoneInfo;
                    SelectedCharacter = mainRowViewModelBase.FightCharacter.Character;
                    break;
                case ViewingMode.DamageTaken:
                    SelectedViewingMode = ViewingMode.DamageTakenInfo;
                    SelectedCharacter = mainRowViewModelBase.FightCharacter.Character;
                    break;
                default: return false;
            }

            SetAndUpdateRows();
            return true;
        }

        public bool TryRegressingView()
        {
            switch (SelectedViewingMode)
            {
                case ViewingMode.DamageDone:
                    SelectedViewingMode = ViewingMode.ViewingModes;
                    break;
                case ViewingMode.DamageDoneInfo:
                    SelectedViewingMode = ViewingMode.DamageDone;
                    SelectedCharacter = null;
                    break;
                case ViewingMode.DamageTaken:
                    SelectedViewingMode = ViewingMode.ViewingModes;
                    break;
                case ViewingMode.DamageTakenInfo:
                    SelectedViewingMode = ViewingMode.DamageTaken;
                    SelectedCharacter = null;
                    break;
                case ViewingMode.OwnersHealingDone:
                    SelectedViewingMode = ViewingMode.ViewingModes;
                    SelectedCharacter = null;
                    break;
                case ViewingMode.OwnersHealingTaken:
                    SelectedViewingMode = ViewingMode.ViewingModes;
                    SelectedCharacter = null;
                    break;
                case ViewingMode.OwnersCasts:
                    SelectedViewingMode = ViewingMode.ViewingModes;
                    SelectedCharacter = null;
                    break;
                default: return false;
            }

            SetAndUpdateRows();
            return true;
        }

        private void SetAndUpdateRows()
        {
            if (DamageMeter == null) return; // Edge case where reporter lags behind cancellation/disposal.

            lock (DamageMeter)
            {
                switch (SelectedViewingMode)
                {
                    case ViewingMode.ViewingModes: SetAndUpdateViewingModeRows(); return;
                    case ViewingMode.DamageDone: SetAndUpdateDamageDoneRows(); return;
                    case ViewingMode.DamageDoneInfo: SetAndUpdateDamageDoneInfoRows(); return;
                    case ViewingMode.DamageTaken: SetAndUpdateDamageTakenRows(); return;
                    case ViewingMode.DamageTakenInfo: SetAndUpdateDamageTakenInfoRows(); return;
                    case ViewingMode.OwnersHealingDone: SetAndUpdateOwnersHealingDoneRows(); return;
                    case ViewingMode.OwnersHealingTaken: SetAndUpdateOwnersHealingTakenRows(); return;
                    case ViewingMode.OwnersCasts: SetAndUpdateOwnersCastsRows(); return;
                    default: throw new NotImplementedException();
                }
            }
        }

        private void SetAndUpdateViewingModeRows()
        {
            if (_viewingModeRows.Count == 0)
            {
                foreach (var viewingModeRow in ViewingModeMainRowBase.GetRows(DamageMeter.CurrentFight))
                {
                    _viewingModeRows.Add(viewingModeRow);
                }
            }

            if (Rows != _viewingModeRows)
            {
                Rows = _viewingModeRows;
            }

            foreach (var viewingModeRow in _viewingModeRows)
            {
                viewingModeRow.Update();
            }
        }

        private void SetAndUpdateDamageDoneRows()
        {
            if (Rows != _damageDoneRows)
            {
                Rows = _damageDoneRows;
            }

            int displayIndex = 1;
            foreach (var fightCharacter in DamageMeter.CurrentFight.FightCharacters
                .Where(c => !c.IsFightPet)
                .OrderByDescending(c => c.TotalDamageDonePlusPets)
                .ThenBy(c => c.UncoloredName))
            {
                if (!Settings.Default.ShowTopLevelNPCRows && fightCharacter.IsNPC
                    || !Settings.Default.ShowTopLevelZeroDamageRows && fightCharacter.TotalDamageDonePlusPets == 0)
                {
                    if (_damageDoneRowMap.TryGetValue(fightCharacter, out MainRowBase damageDoneRow))
                    {
                        _damageDoneRowMap.Remove(fightCharacter);
                        _damageDoneRows.Remove(damageDoneRow);
                    }
                }
                else
                {
                    if (!_damageDoneRowMap.TryGetValue(fightCharacter, out MainRowBase damageDoneRow))
                    {
                        _damageDoneRowMap.Add(fightCharacter, damageDoneRow = new DamageDoneMainRow(fightCharacter));
                        _damageDoneRows.Add(damageDoneRow);
                    }
                    damageDoneRow.Update(displayIndex++);
                }
            }
        }

        private void SetAndUpdateDamageDoneInfoRows()
        {
            if (!DamageMeter.CurrentFight.TryGetFightCharacter(SelectedCharacter, out FightCharacter fightCharacter))
                return;

            Dictionary<DamageInfo, MainRowBase> damageDoneInfoRowMap;
            ObservableCollection<MainRowBase> damageDoneInfoRows;
            if (_damageDoneInfoRowMapMap.TryGetValue(fightCharacter, out damageDoneInfoRowMap))
            {
                damageDoneInfoRows = _damageDoneInfoRowsMap[fightCharacter];

                if (Rows != damageDoneInfoRows)
                {
                    Rows = damageDoneInfoRows;
                }
            }
            else
            {
                _damageDoneInfoRowMapMap[fightCharacter] = damageDoneInfoRowMap = new Dictionary<DamageInfo, MainRowBase>();
                Rows = _damageDoneInfoRowsMap[fightCharacter] = damageDoneInfoRows = new ObservableCollection<MainRowBase>();
            }

            int displayIndex = 1;
            foreach (var damageDoneInfo in fightCharacter.DamageDoneInfos
                .OrderByDescending(i => i.TotalDamagePlusPets)
                .ThenBy(i => i.Target.UncoloredName))
            {
                if (!damageDoneInfoRowMap.TryGetValue(damageDoneInfo, out MainRowBase damageDoneInfoRow))
                {
                    damageDoneInfoRowMap.Add(damageDoneInfo, damageDoneInfoRow = new DamageDoneInfoMainRow(damageDoneInfo));
                    damageDoneInfoRows.Add(damageDoneInfoRow);
                }
                damageDoneInfoRow.Update(displayIndex++);
            }
        }

        private void SetAndUpdateDamageTakenRows()
        {
            if (Rows != _damageTakenRows)
            {
                Rows = _damageTakenRows;
            }

            int displayIndex = 1;
            foreach (var fightCharacter in DamageMeter.CurrentFight.FightCharacters
                .OrderByDescending(c => c.TotalDamageTaken)
                .ThenBy(c => c.UncoloredName))
            {
                if (!Settings.Default.ShowTopLevelNPCRows && fightCharacter.IsNPC
                    || !Settings.Default.ShowTopLevelZeroDamageRows && fightCharacter.TotalDamageTaken == 0)
                {
                    if (_damageTakenRowMap.TryGetValue(fightCharacter, out MainRowBase damageTakenRow))
                    {
                        _damageTakenRowMap.Remove(fightCharacter);
                        _damageTakenRows.Remove(damageTakenRow);
                    }
                }
                else
                {
                    if (!_damageTakenRowMap.TryGetValue(fightCharacter, out MainRowBase damageTakenRow))
                    {
                        _damageTakenRowMap.Add(fightCharacter, damageTakenRow = new DamageTakenMainRow(fightCharacter));
                        _damageTakenRows.Add(damageTakenRow);
                    }
                    damageTakenRow.Update(displayIndex++);
                }
            }
        }

        private void SetAndUpdateDamageTakenInfoRows()
        {
            if (!DamageMeter.CurrentFight.TryGetFightCharacter(SelectedCharacter, out FightCharacter fightCharacter))
                return;

            Dictionary<DamageInfo, MainRowBase> damageTakenInfoRowMap;
            ObservableCollection<MainRowBase> damageTakenInfoRows;
            if (_damageTakenInfoRowMapMap.TryGetValue(fightCharacter, out damageTakenInfoRowMap))
            {
                damageTakenInfoRows = _damageTakenInfoRowsMap[fightCharacter];

                if (Rows != damageTakenInfoRows)
                {
                    Rows = damageTakenInfoRows;
                }
            }
            else
            {
                _damageTakenInfoRowMapMap[fightCharacter] = damageTakenInfoRowMap = new Dictionary<DamageInfo, MainRowBase>();
                Rows = _damageTakenInfoRowsMap[fightCharacter] = damageTakenInfoRows = new ObservableCollection<MainRowBase>();
            }

            int displayIndex = 1;
            foreach (var damageTakenInfo in fightCharacter.DamageTakenInfos
                .Where(i => !i.Source.IsFightPet)
                .OrderByDescending(i => i.TotalDamagePlusPets)
                .ThenBy(i => i.Source.UncoloredName))
            {
                if (!damageTakenInfoRowMap.TryGetValue(damageTakenInfo, out MainRowBase damageTakenInfoRow))
                {
                    damageTakenInfoRowMap.Add(damageTakenInfo, damageTakenInfoRow = new DamageTakenInfoMainRow(damageTakenInfo));
                    damageTakenInfoRows.Add(damageTakenInfoRow);
                }
                damageTakenInfoRow.Update(displayIndex++);
            }
        }

        private void SetAndUpdateOwnersHealingDoneRows()
        {
            if (Rows != _ownersHealingDoneRows)
            {
                Rows = _ownersHealingDoneRows;
            }

            if (!DamageMeter.CurrentFight.TryGetFightOwnerCharacter(out FightCharacter fightOwner))
                return;

            int displayIndex = 1;
            foreach (var healingDoneInfo in fightOwner.HealingDoneInfos
                .OrderByDescending(i => i.PotentialHealingPlusPets)
                .ThenBy(i => i.Target.UncoloredName))
            {
                if (!_ownersHealingDoneRowMap.TryGetValue(healingDoneInfo, out MainRowBase ownersHealingDoneRow))
                {
                    _ownersHealingDoneRowMap.Add(healingDoneInfo, ownersHealingDoneRow = new OwnersHealingDoneMainRow(healingDoneInfo));
                    _ownersHealingDoneRows.Add(ownersHealingDoneRow);
                }
                ownersHealingDoneRow.Update(displayIndex++);
            }
        }

        private void SetAndUpdateOwnersHealingTakenRows()
        {
            if (Rows != _ownersHealingTakenRows)
            {
                Rows = _ownersHealingTakenRows;
            }

            if (!DamageMeter.CurrentFight.TryGetFightOwnerCharacter(out FightCharacter fightOwner))
                return;

            int displayIndex = 1;
            foreach (var healingTakenInfo in fightOwner.HealingTakenInfos
                .Where(i => !i.Source.IsFightPet)
                .OrderByDescending(i => i.PotentialHealingPlusPets)
                .ThenBy(i => i.Target.UncoloredName))
            {
                if (!_ownersHealingTakenRowMap.TryGetValue(healingTakenInfo, out MainRowBase ownersHealingTakenRow))
                {
                    _ownersHealingTakenRowMap.Add(healingTakenInfo, ownersHealingTakenRow = new OwnersHealingTakenMainRow(healingTakenInfo));
                    _ownersHealingTakenRows.Add(ownersHealingTakenRow);
                }
                ownersHealingTakenRow.Update(displayIndex++);
            }
        }

        private void SetAndUpdateOwnersCastsRows()
        {
            if (Rows != _ownersCastsRows)
            {
                Rows = _ownersCastsRows;
            }

            if (!DamageMeter.CurrentFight.TryGetFightOwnerCharacter(out FightCharacter fightOwner))
                return;

            int displayIndex = 1;
            foreach (var castInfo in fightOwner.CastInfos
                .OrderByDescending(i => i.CastSuccesses)
                .ThenBy(i => i.NanoProgram))
            {
                if (!_ownersCastsRowMap.TryGetValue(castInfo, out MainRowBase ownersCastsRow))
                {
                    _ownersCastsRowMap.Add(castInfo, ownersCastsRow = new OwnersCastsMainRow(castInfo));
                    _ownersCastsRows.Add(ownersCastsRow);
                }
                ownersCastsRow.Update(displayIndex++);
            }
        }

        private void StopDamageMeterUpdater()
        {
            if (!_isDamageMeterUpdaterStarted) return;

            _isDamageMeterUpdaterStarted = false;
            _damageMeterUpdaterCTS.Cancel();
            _damageMeterUpdater.Wait();
            _damageMeterUpdaterCTS.Dispose();
        }

        public ICommand ResetDamageMeterCommand { get; }
        private void ExecuteResetDamageMeterCommand()
        {
            if (DamageMeter == null) return;

            StopDamageMeterUpdater();
            ClearRows();

            DamageMeter.InitializeNewFight();
            StartDamageMeterUpdater();
        }

        public ICommand ToggleIsPausedCommand { get; }
        private void ExecuteToggleIsPausedCommand()
            => IsPaused = !IsPaused;

        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                Set(ref _isPaused, value);

                if (DamageMeter == null) return;
                lock (DamageMeter)
                {
                    DamageMeter.IsPaused = IsPaused;
                }
            }
        }

        private void ClearRows()
        {
            _viewingModeRows.Clear();
            _damageDoneRowMap.Clear();
            _damageDoneRows.Clear();
            _damageDoneInfoRowMapMap.Clear();
            _damageDoneInfoRowsMap.Clear();
            _damageTakenRowMap.Clear();
            _damageTakenRows.Clear();
            _damageTakenInfoRowMapMap.Clear();
            _damageTakenInfoRowsMap.Clear();
            _ownersHealingDoneRowMap.Clear();
            _ownersHealingDoneRows.Clear();
            _ownersHealingTakenRowMap.Clear();
            _ownersHealingTakenRows.Clear();
            _ownersCastsRowMap.Clear();
            _ownersCastsRows.Clear();
            Rows = null;
        }

        public void DisposeDamageMeter()
        {
            StopDamageMeterUpdater();
            DamageMeter?.Dispose();
            DamageMeter = null;
        }
    }
}
