using AODamageMeter.UI.Properties;
using AODamageMeter.UI.ViewModels.Rows;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AODamageMeter.UI.ViewModels
{
    public sealed class DamageMeterViewModel : ViewModelBase
    {
        private readonly IProgress<object> _rowUpdater;
        private string _characterName;
        private string _logFilePath;
        private CancellationTokenSource _damageMeterUpdaterCTS;
        private Task _damageMeterUpdater;
        private bool _isDamageMeterUpdaterStarted;
        private readonly ObservableCollection<MainRowBase> _fightRows = new ObservableCollection<MainRowBase>();
        private bool _historicalFightRowsNeedUpdating = false;

        public DamageMeterViewModel()
        {
            _rowUpdater = new Progress<object>(_ => UpdateDisplayedRows());
            ClearFightHistoryCommand = new RelayCommand(CanExecuteClearFightHistoryCommand, ExecuteClearFightHistoryCommand);
            ToggleIsPausedCommand = new RelayCommand(ExecuteToggleIsPausedCommand);
            ResetAndSaveFightCommand = new RelayCommand(ExecuteResetAndSaveFightCommand);
            ResetFightCommand = new RelayCommand(ExecuteResetFightCommand);
            TryInitializeDamageMeter(Settings.Default.SelectedCharacterName, Settings.Default.SelectedLogFilePath);
            // Performance optimization: don't let performance degrade as the # of fights in the fight history increase.
            // Would be more complicated to extend this to views within a historical fight, but possible. But don't need
            // to do it there because it's less expensive than the unavoidable cost of updating the current fight.
            Settings.Default.PropertyChanged += (_, e) =>
                _historicalFightRowsNeedUpdating = _historicalFightRowsNeedUpdating
                    || e.PropertyName == nameof(Settings.Default.ShowRowNumbers)
                    || e.PropertyName == nameof(Settings.Default.ShowPercentOfTotal)
                    || e.PropertyName == nameof(Settings.Default.IncludeTopLevelZeroDamageRows)
                    || e.PropertyName == nameof(Settings.Default.IncludeTopLevelNPCRows);
        }

        public DamageMeter DamageMeter { get; private set; }
        public Character Owner => DamageMeter?.Owner;
        public Fight CurrentFight => DamageMeter?.CurrentFight;

        private ViewingMode SelectedViewingMode { get; set; } = ViewingMode.Fight;
        private FightViewModel CurrentFightViewModel { get; set; }
        private FightViewModel SelectedFightViewModel { get; set; }
        private Character SelectedCharacter { get; set; }

        public string Title
        {
            get
            {
                switch (SelectedViewingMode)
                {
                    case ViewingMode.Fights: return "Fights";
                    case ViewingMode.Fight: return SelectedFightViewModel?.FightTitle ?? "Fight";
                    case ViewingMode.DamageDone: return "Damage Done";
                    case ViewingMode.DamageDoneInfo: return $"{SelectedCharacter.UncoloredName}'s Damage Done";
                    case ViewingMode.DamageTaken: return "Damage Taken";
                    case ViewingMode.DamageTakenInfo: return $"{SelectedCharacter.UncoloredName}'s Damage Taken";
                    case ViewingMode.OwnersHealingDone: return $"{SelectedCharacter.UncoloredName}'s Healing Done";
                    case ViewingMode.OwnersHealingTaken: return $"{SelectedCharacter.UncoloredName}'s Healing Taken";
                    case ViewingMode.OwnersCasts: return $"{SelectedCharacter.UncoloredName}'s Casts";
                    default: throw new NotImplementedException();
                }
            }
        }

        private ObservableCollection<MainRowBase> _displayedRows;
        public ObservableCollection<MainRowBase> DisplayedRows
        {
            get => _displayedRows;
            private set => Set(ref _displayedRows, value);
        }

        public bool TryInitializeDamageMeter(string characterName, string logFilePath)
        {
            if (string.IsNullOrWhiteSpace(characterName) || string.IsNullOrWhiteSpace(logFilePath))
                return false;

            if (characterName == _characterName && logFilePath == _logFilePath)
                return true;

            if (!File.Exists(logFilePath))
            {
                try { File.Create(logFilePath); }
                catch { return false; }
            }

            DisposeDamageMeter();

            _characterName = characterName;
            _logFilePath = logFilePath;
            DamageMeter = new DamageMeter(characterName, logFilePath) { IsPaused = IsPaused };
            StartNewFight();

            // If the view was scoped to the old owner, update to the new owner so the view's title updates.
            if (SelectedViewingMode == ViewingMode.OwnersHealingDone
                || SelectedViewingMode == ViewingMode.OwnersHealingTaken
                || SelectedViewingMode == ViewingMode.OwnersCasts)
            {
                SelectedCharacter = Owner;
                RaisePropertyChanged(nameof(Title));
            }

            return true;
        }

        private void StartNewFight(bool saveCurrentFight = false)
        {
            StopDamageMeterUpdater();

#if DEBUG
            DamageMeter.InitializeNewFight(skipToEndOfLog: false, saveCurrentFight: saveCurrentFight);
#else
            DamageMeter.InitializeNewFight(saveCurrentFight: saveCurrentFight);
#endif
            if (CurrentFightViewModel != null)
            {
                if (saveCurrentFight)
                {
                    // Final update before this fight row gets updated only when settings change (see constructor above). We're
                    // doing this after InitializeNewFight because that's when this fight gets paused & its stats set in stone.
                    _fightRows[_fightRows.Count - 1].Update();
                }
                else
                {
                    _fightRows.RemoveAt(_fightRows.Count - 1);
                }
            }

            CurrentFightViewModel = new FightViewModel(CurrentFight);
            SelectedFightViewModel = SelectedViewingMode != ViewingMode.Fights ? CurrentFightViewModel : null;
            _fightRows.Add(new FightMainRow(CurrentFightViewModel, displayIndex: _fightRows.Count + 1));

            StartDamageMeterUpdater();
        }

        private void StopDamageMeterUpdater()
        {
            if (!_isDamageMeterUpdaterStarted) return;

            _isDamageMeterUpdaterStarted = false;
            _damageMeterUpdaterCTS.Cancel();
            _damageMeterUpdater.Wait();
            _damageMeterUpdaterCTS.Dispose();
        }

        private void StartDamageMeterUpdater()
        {
            if (_isDamageMeterUpdaterStarted) return;

            _damageMeterUpdaterCTS = new CancellationTokenSource();
            _damageMeterUpdater = Task.Factory.StartNew(() =>
            {
                do
                {
                    lock (CurrentFight)
                    {
                        DamageMeter.Update();
                    }
                    if (_damageMeterUpdaterCTS.IsCancellationRequested) return;
                    _rowUpdater.Report(null);
                } while (!_damageMeterUpdaterCTS.Token.WaitHandle.WaitOne(Settings.Default.RefreshInterval));
            }, _damageMeterUpdaterCTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _isDamageMeterUpdaterStarted = true;
        }

        public bool TryProgressView(MainRowBase mainRow)
        {
            switch (SelectedViewingMode)
            {
                case ViewingMode.Fights:
                    SelectedViewingMode = ViewingMode.Fight;
                    SelectedFightViewModel = mainRow.FightViewModel;
                    break;
                case ViewingMode.Fight:
                    var viewingMode = ((ViewingModeMainRowBase)mainRow).ViewingMode;
                    if (viewingMode == ViewingMode.OwnersXP) return false;

                    SelectedViewingMode = viewingMode;
                    if (SelectedViewingMode == ViewingMode.OwnersHealingDone
                        || SelectedViewingMode == ViewingMode.OwnersHealingTaken
                        || SelectedViewingMode == ViewingMode.OwnersCasts)
                    {
                        SelectedCharacter = Owner;
                    }
                    break;
                case ViewingMode.DamageDone:
                    SelectedViewingMode = ViewingMode.DamageDoneInfo;
                    SelectedCharacter = ((FightCharacterMainRowBase)mainRow).FightCharacter.Character;
                    break;
                case ViewingMode.DamageTaken:
                    SelectedViewingMode = ViewingMode.DamageTakenInfo;
                    SelectedCharacter = ((FightCharacterMainRowBase)mainRow).FightCharacter.Character;
                    break;
                default: return false;
            }

            RaisePropertyChanged(nameof(Title));
            UpdateDisplayedRows();

            return true;
        }

        public bool TryRegressView()
        {
            switch (SelectedViewingMode)
            {
                case ViewingMode.Fight:
                    SelectedViewingMode = ViewingMode.Fights;
                    SelectedFightViewModel = null;
                    break;
                case ViewingMode.DamageDone:
                    SelectedViewingMode = ViewingMode.Fight;
                    break;
                case ViewingMode.DamageDoneInfo:
                    SelectedViewingMode = ViewingMode.DamageDone;
                    SelectedCharacter = null;
                    break;
                case ViewingMode.DamageTaken:
                    SelectedViewingMode = ViewingMode.Fight;
                    break;
                case ViewingMode.DamageTakenInfo:
                    SelectedViewingMode = ViewingMode.DamageTaken;
                    SelectedCharacter = null;
                    break;
                case ViewingMode.OwnersHealingDone:
                    SelectedViewingMode = ViewingMode.Fight;
                    SelectedCharacter = null;
                    break;
                case ViewingMode.OwnersHealingTaken:
                    SelectedViewingMode = ViewingMode.Fight;
                    SelectedCharacter = null;
                    break;
                case ViewingMode.OwnersCasts:
                    SelectedViewingMode = ViewingMode.Fight;
                    SelectedCharacter = null;
                    break;
                default: return false;
            }

            RaisePropertyChanged(nameof(Title));
            UpdateDisplayedRows();

            return true;
        }

        private void UpdateDisplayedRows()
        {
            if (DamageMeter == null) return; // Edge case where reporter lags behind cancellation/disposal.

            ObservableCollection<MainRowBase> updatedRows;
            switch (SelectedViewingMode)
            {
                case ViewingMode.Fights: updatedRows = GetUpdatedFightRows(); break;
                case ViewingMode.Fight:
                    updatedRows = SelectedFightViewModel.GetUpdatedViewingModeRows();
                    if (SelectedFightViewModel == CurrentFightViewModel)
                    {
                        RaisePropertyChanged(nameof(Title));
                    }
                    break;
                case ViewingMode.DamageDone: updatedRows = SelectedFightViewModel.GetUpdatedDamageDoneRows(); break;
                case ViewingMode.DamageDoneInfo: updatedRows = SelectedFightViewModel.GetUpdatedDamageDoneInfoRows(SelectedCharacter); break;
                case ViewingMode.DamageTaken: updatedRows = SelectedFightViewModel.GetUpdatedDamageTakenRows(); break;
                case ViewingMode.DamageTakenInfo: updatedRows = SelectedFightViewModel.GetUpdatedDamageTakenInfoRows(SelectedCharacter); break;
                case ViewingMode.OwnersHealingDone: updatedRows = SelectedFightViewModel.GetUpdatedOwnersHealingDoneRows(); break;
                case ViewingMode.OwnersHealingTaken: updatedRows = SelectedFightViewModel.GetUpdatedOwnersHealingTakenRows(); break;
                case ViewingMode.OwnersCasts: updatedRows = SelectedFightViewModel.GetUpdatedOwnersCastsRows(); break;
                default: throw new NotImplementedException();
            }

            if (DisplayedRows != updatedRows)
            {
                DisplayedRows = updatedRows;
            }
        }

        private ObservableCollection<MainRowBase> GetUpdatedFightRows()
        {
            lock (CurrentFight)
            {
                if (_historicalFightRowsNeedUpdating)
                {
                    foreach (var fightRow in _fightRows)
                    {
                        fightRow.Update();
                    }

                    _historicalFightRowsNeedUpdating = false;
                }
                else
                {
                    _fightRows[_fightRows.Count - 1].Update();
                }

                return _fightRows;
            }
        }

        public ICommand ClearFightHistoryCommand { get; }
        private bool CanExecuteClearFightHistoryCommand()
            => _fightRows.Count > 1 && SelectedViewingMode == ViewingMode.Fights;
        private void ExecuteClearFightHistoryCommand()
        {
            for (int i = _fightRows.Count - 2; i >= 0; --i)
            {
                _fightRows.RemoveAt(i);
            }
            _fightRows[0].Update(displayIndex: 1);
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

                if (CurrentFight == null) return;
                lock (CurrentFight)
                {
                    DamageMeter.IsPaused = IsPaused;
                }
            }
        }

        public ICommand ResetAndSaveFightCommand { get; }
        private void ExecuteResetAndSaveFightCommand()
        {
            if (DamageMeter == null) return;

            StartNewFight(saveCurrentFight: CurrentFight?.HasStarted ?? false);
        }

        public ICommand ResetFightCommand { get; }
        private void ExecuteResetFightCommand()
        {
            if (DamageMeter == null) return;

            StartNewFight();
        }

        public void DisposeDamageMeter()
        {
            if (DamageMeter == null) return;

            StopDamageMeterUpdater();
            DamageMeter.Dispose();
            DamageMeter = null;
        }
    }
}
