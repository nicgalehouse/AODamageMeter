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
            _rowUpdater = new Progress<object>(_ => UpdateDisplayedRows());
            TryInitializeDamageMeter(Settings.Default.SelectedCharacterName, Settings.Default.SelectedLogFilePath);
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
        private ObservableCollection<MainRowBase> _displayedRows;
        public ObservableCollection<MainRowBase> DisplayedRows
        {
            get => _displayedRows;
            set => Set(ref _displayedRows, value);
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

        public bool TryInitializeDamageMeter(string characterName, string logFilePath)
        {
            // No reason to reinitialize if same name/path AND we succeeded before (i.e., _damageMeter is not null).
            if (_characterName == characterName && _logFilePath == logFilePath && DamageMeter != null)
                return true;

            _characterName = characterName;
            _logFilePath = logFilePath;

            DisposeDamageMeter();

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
                } while (!_damageMeterUpdaterCTS.Token.WaitHandle.WaitOne(Settings.Default.RefreshInterval));
            }, _damageMeterUpdaterCTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _isDamageMeterUpdaterStarted = true;
        }

        public bool TryProgressView(MainRowBase mainRowViewModelBase)
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
                    SelectedCharacter = ((FightCharacterMainRowBase)mainRowViewModelBase).FightCharacter.Character;
                    break;
                case ViewingMode.DamageTaken:
                    SelectedViewingMode = ViewingMode.DamageTakenInfo;
                    SelectedCharacter = ((FightCharacterMainRowBase)mainRowViewModelBase).FightCharacter.Character;
                    break;
                default: return false;
            }

            UpdateDisplayedRows();

            return true;
        }

        public bool TryRegressView()
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

            UpdateDisplayedRows();

            return true;
        }

        private void UpdateDisplayedRows()
        {
            if (DamageMeter == null) return; // Edge case where reporter lags behind cancellation/disposal.

            ObservableCollection<MainRowBase> updatedRows;
            switch (SelectedViewingMode)
            {
                case ViewingMode.ViewingModes: updatedRows = GetUpdatedViewingModeRows(); break;
                case ViewingMode.DamageDone: updatedRows = GetUpdatedDamageDoneRows(); break;
                case ViewingMode.DamageDoneInfo: updatedRows = GetUpdatedDamageDoneInfoRows(SelectedCharacter); break;
                case ViewingMode.DamageTaken: updatedRows = GetUpdatedDamageTakenRows(); break;
                case ViewingMode.DamageTakenInfo: updatedRows = GetUpdatedDamageTakenInfoRows(SelectedCharacter); break;
                case ViewingMode.OwnersHealingDone: updatedRows = GetUpdatedOwnersHealingDoneRows(); break;
                case ViewingMode.OwnersHealingTaken: updatedRows = GetUpdatedOwnersHealingTakenRows(); break;
                case ViewingMode.OwnersCasts: updatedRows = GetUpdatedOwnersCastsRows(); break;
                default: throw new NotImplementedException();
            }

            if (DisplayedRows != updatedRows)
            {
                DisplayedRows = updatedRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedViewingModeRows()
        {
            lock (DamageMeter)
            {
                if (_viewingModeRows.Count == 0)
                {
                    foreach (var viewingModeRow in ViewingModeMainRowBase.GetRows(this, DamageMeter.CurrentFight))
                    {
                        _viewingModeRows.Add(viewingModeRow);
                    }
                }

                foreach (var viewingModeRow in _viewingModeRows)
                {
                    viewingModeRow.Update();
                }

                return _viewingModeRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageDoneRows()
        {
            lock (DamageMeter)
            {
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
                            _damageDoneRowMap.Add(fightCharacter, damageDoneRow = new DamageDoneMainRow(this, fightCharacter));
                            _damageDoneRows.Add(damageDoneRow);
                        }
                        damageDoneRow.Update(displayIndex++);
                    }
                }

                return _damageDoneRows;
            }
        }

        private ObservableCollection<MainRowBase> GetUpdatedDamageDoneInfoRows(Character character)
        {
            lock (DamageMeter)
            {
                return DamageMeter.CurrentFight.TryGetFightCharacter(SelectedCharacter, out FightCharacter fightCharacter)
                    ? GetUpdatedDamageDoneInfoRows(fightCharacter) : null;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageDoneInfoRows(FightCharacter fightCharacter)
        {
            lock (DamageMeter)
            {
                Dictionary<DamageInfo, MainRowBase> damageDoneInfoRowMap;
                ObservableCollection<MainRowBase> damageDoneInfoRows;
                if (_damageDoneInfoRowMapMap.TryGetValue(fightCharacter, out damageDoneInfoRowMap))
                {
                    damageDoneInfoRows = _damageDoneInfoRowsMap[fightCharacter];
                }
                else
                {
                    damageDoneInfoRowMap = _damageDoneInfoRowMapMap[fightCharacter] = new Dictionary<DamageInfo, MainRowBase>();
                    damageDoneInfoRows = _damageDoneInfoRowsMap[fightCharacter] = new ObservableCollection<MainRowBase>();
                }

                int displayIndex = 1;
                foreach (var damageDoneInfo in fightCharacter.DamageDoneInfos
                    .OrderByDescending(i => i.TotalDamagePlusPets)
                    .ThenBy(i => i.Target.UncoloredName))
                {
                    if (!damageDoneInfoRowMap.TryGetValue(damageDoneInfo, out MainRowBase damageDoneInfoRow))
                    {
                        damageDoneInfoRowMap.Add(damageDoneInfo, damageDoneInfoRow = new DamageDoneInfoMainRow(this, damageDoneInfo));
                        damageDoneInfoRows.Add(damageDoneInfoRow);
                    }
                    damageDoneInfoRow.Update(displayIndex++);
                }

                return damageDoneInfoRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageTakenRows()
        {
            lock (DamageMeter)
            {
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
                            _damageTakenRowMap.Add(fightCharacter, damageTakenRow = new DamageTakenMainRow(this, fightCharacter));
                            _damageTakenRows.Add(damageTakenRow);
                        }
                        damageTakenRow.Update(displayIndex++);
                    }
                }

                return _damageTakenRows;
            }
        }

        private ObservableCollection<MainRowBase> GetUpdatedDamageTakenInfoRows(Character character)
        {
            lock (DamageMeter)
            {
                return DamageMeter.CurrentFight.TryGetFightCharacter(SelectedCharacter, out FightCharacter fightCharacter)
                    ? GetUpdatedDamageTakenInfoRows(fightCharacter) : null;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedDamageTakenInfoRows(FightCharacter fightCharacter)
        {
            lock (DamageMeter)
            {
                Dictionary<DamageInfo, MainRowBase> damageTakenInfoRowMap;
                ObservableCollection<MainRowBase> damageTakenInfoRows;
                if (_damageTakenInfoRowMapMap.TryGetValue(fightCharacter, out damageTakenInfoRowMap))
                {
                    damageTakenInfoRows = _damageTakenInfoRowsMap[fightCharacter];
                }
                else
                {
                    damageTakenInfoRowMap = _damageTakenInfoRowMapMap[fightCharacter] = new Dictionary<DamageInfo, MainRowBase>();
                    damageTakenInfoRows = _damageTakenInfoRowsMap[fightCharacter] = new ObservableCollection<MainRowBase>();
                }

                int displayIndex = 1;
                foreach (var damageTakenInfo in fightCharacter.DamageTakenInfos
                    .Where(i => !i.Source.IsFightPet)
                    .OrderByDescending(i => i.TotalDamagePlusPets)
                    .ThenBy(i => i.Source.UncoloredName))
                {
                    if (!damageTakenInfoRowMap.TryGetValue(damageTakenInfo, out MainRowBase damageTakenInfoRow))
                    {
                        damageTakenInfoRowMap.Add(damageTakenInfo, damageTakenInfoRow = new DamageTakenInfoMainRow(this, damageTakenInfo));
                        damageTakenInfoRows.Add(damageTakenInfoRow);
                    }
                    damageTakenInfoRow.Update(displayIndex++);
                }

                return damageTakenInfoRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedOwnersHealingDoneRows()
        {
            lock (DamageMeter)
            {
                if (!DamageMeter.CurrentFight.TryGetFightOwner(out FightCharacter fightOwner))
                    return _ownersHealingDoneRows;

                int displayIndex = 1;
                foreach (var healingDoneInfo in fightOwner.HealingDoneInfos
                    .OrderByDescending(i => i.PotentialHealingPlusPets)
                    .ThenBy(i => i.Target.UncoloredName))
                {
                    if (!_ownersHealingDoneRowMap.TryGetValue(healingDoneInfo, out MainRowBase ownersHealingDoneRow))
                    {
                        _ownersHealingDoneRowMap.Add(healingDoneInfo, ownersHealingDoneRow = new OwnersHealingDoneMainRow(this, healingDoneInfo));
                        _ownersHealingDoneRows.Add(ownersHealingDoneRow);
                    }
                    ownersHealingDoneRow.Update(displayIndex++);
                }

                return _ownersHealingDoneRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedOwnersHealingTakenRows()
        {
            lock (DamageMeter)
            {
                if (!DamageMeter.CurrentFight.TryGetFightOwner(out FightCharacter fightOwner))
                    return _ownersHealingTakenRows;

                int displayIndex = 1;
                foreach (var healingTakenInfo in fightOwner.HealingTakenInfos
                    .Where(i => !i.Source.IsFightPet)
                    .OrderByDescending(i => i.PotentialHealingPlusPets)
                    .ThenBy(i => i.Target.UncoloredName))
                {
                    if (!_ownersHealingTakenRowMap.TryGetValue(healingTakenInfo, out MainRowBase ownersHealingTakenRow))
                    {
                        _ownersHealingTakenRowMap.Add(healingTakenInfo, ownersHealingTakenRow = new OwnersHealingTakenMainRow(this, healingTakenInfo));
                        _ownersHealingTakenRows.Add(ownersHealingTakenRow);
                    }
                    ownersHealingTakenRow.Update(displayIndex++);
                }

                return _ownersHealingTakenRows;
            }
        }

        public ObservableCollection<MainRowBase> GetUpdatedOwnersCastsRows()
        {
            lock (DamageMeter)
            {
                if (!DamageMeter.CurrentFight.TryGetFightOwner(out FightCharacter fightOwner))
                    return _ownersCastsRows;

                int displayIndex = 1;
                foreach (var castInfo in fightOwner.CastInfos
                    .OrderByDescending(i => i.CastSuccesses)
                    .ThenBy(i => i.NanoProgram))
                {
                    if (!_ownersCastsRowMap.TryGetValue(castInfo, out MainRowBase ownersCastsRow))
                    {
                        _ownersCastsRowMap.Add(castInfo, ownersCastsRow = new OwnersCastsMainRow(this, castInfo));
                        _ownersCastsRows.Add(ownersCastsRow);
                    }
                    ownersCastsRow.Update(displayIndex++);
                }

                return _ownersCastsRows;
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

            lock (DamageMeter)
            {
                ClearRows();
                DamageMeter.InitializeNewFight();
                StartDamageMeterUpdater();
            }
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

        public void DisposeDamageMeter()
        {
            if (DamageMeter == null) return;

            StopDamageMeterUpdater();

            lock (DamageMeter)
            {
                ClearRows();
                DamageMeter.Dispose();
                DamageMeter = null;
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
            DisplayedRows = null;
        }
    }
}
