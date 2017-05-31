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
    public class DamageMeterViewModel : ViewModelBase
    {
        private string _characterName;
        private string _logFilePath;
        private DamageMeter _damageMeter;
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

        private Dictionary<FightCharacter, MainRowViewModelBase> _damageDoneRowMap = new Dictionary<FightCharacter, MainRowViewModelBase>();
        private ObservableCollection<MainRowViewModelBase> _damageDoneRows { get; } = new ObservableCollection<MainRowViewModelBase>();

        private Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowViewModelBase>> _damageDoneInfoRowMapMap = new Dictionary<FightCharacter, Dictionary<DamageInfo, MainRowViewModelBase>>();
        private Dictionary<FightCharacter, ObservableCollection<MainRowViewModelBase>> _damageDoneInfoRowsMap = new Dictionary<FightCharacter, ObservableCollection<MainRowViewModelBase>>();

        private ObservableCollection<MainRowViewModelBase> _rows;
        public ObservableCollection<MainRowViewModelBase> Rows
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

        public bool TryProgressingView(FightCharacter fightCharacter)
        {
            if (SelectedViewingMode == ViewingMode.DamageDone && fightCharacter != null)
            {
                SelectedViewingMode = ViewingMode.DamageDoneInfo;
                SelectedCharacter = fightCharacter.Character;
                SetAndUpdateRows();
                return true;
            }

            return false;
        }

        public bool TryRegressingView()
        {
            if (SelectedViewingMode == ViewingMode.DamageDoneInfo)
            {
                SelectedViewingMode = ViewingMode.DamageDone;
                SelectedCharacter = null;
                SetAndUpdateRows();
                return true;
            }

            return false;
        }

        public bool TryInitializingDamageMeter(string characterName, string logFilePath)
        {
            // No reason to reinitialize if same name/path AND we succeeded before (i.e., _damageMeter is not null).
            if (_characterName == characterName && _logFilePath == logFilePath && _damageMeter != null)
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

            _damageMeter = Character.FitsPlayerNamingRequirements(characterName)
                ? new DamageMeter(characterName, logFilePath)
                : new DamageMeter(logFilePath);
            _damageMeter.IsPaused = IsPaused;
#if DEBUG
            _damageMeter.InitializeNewFight(skipToEndOfLog: false);
#else
            _damageMeter.InitializeNewFight();
#endif
            StartDamageMeterUpdater();

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
                    lock (_damageMeter)
                    {
                        _damageMeter.Update().Wait();
                    }
                    if (_damageMeterUpdaterCTS.IsCancellationRequested) return;
                    _rowUpdater.Report(null);
                } while (!_damageMeterUpdaterCTS.Token.WaitHandle.WaitOne(300));
            }, _damageMeterUpdaterCTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _isDamageMeterUpdaterStarted = true;
        }

        private void SetAndUpdateRows()
        {
            if (_damageMeter == null) return; // Edge case where reporter lags behind cancellation/disposal.

            lock (_damageMeter)
            {
                switch (SelectedViewingMode)
                {
                    case ViewingMode.DamageDone: SetAndUpdateDamageDoneRows(); return;
                    case ViewingMode.DamageDoneInfo: SetAndUpdateDamageDoneInfoRows(); return;
                    default: throw new NotImplementedException();
                }
            }
        }

        private void SetAndUpdateDamageDoneRows()
        {
            if (Rows != _damageDoneRows)
            {
                Rows = _damageDoneRows;
            }

            int displayIndex = 1;
            foreach (var fightCharacter in _damageMeter.CurrentFight.FightCharacters
                .Where(c => !c.IsPet)
                .OrderByDescending(c => c.TotalDamageDonePlusPets)
                .ThenBy(c => c.UncoloredName))
            {
                if (!_damageDoneRowMap.TryGetValue(fightCharacter, out MainRowViewModelBase damageDoneRow))
                {
                    _damageDoneRowMap.Add(fightCharacter, damageDoneRow = new DamageDoneMainRowViewModel(fightCharacter));
                    _damageDoneRows.Add(damageDoneRow);
                }
                damageDoneRow.Update(displayIndex++);
            }
        }

        private void SetAndUpdateDamageDoneInfoRows()
        {
            if (!_damageMeter.CurrentFight.TryGetFightCharacter(SelectedCharacter, out FightCharacter fightCharacter))
                return;

            Dictionary<DamageInfo, MainRowViewModelBase> damageDoneInfoRowMap;
            ObservableCollection<MainRowViewModelBase> damageDoneInfoRows;
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
                _damageDoneInfoRowMapMap[fightCharacter] = damageDoneInfoRowMap = new Dictionary<DamageInfo, MainRowViewModelBase>();
                Rows = _damageDoneInfoRowsMap[fightCharacter] = damageDoneInfoRows = new ObservableCollection<MainRowViewModelBase>();
            }

            int displayIndex = 1;
            foreach (var damageDoneInfo in fightCharacter.DamageDoneInfos
                .OrderByDescending(i => i.TotalDamagePlusPets)
                .ThenBy(i => i.Target.UncoloredName))
            {
                if (!damageDoneInfoRowMap.TryGetValue(damageDoneInfo, out MainRowViewModelBase damageDoneInfoRow))
                {
                    damageDoneInfoRowMap.Add(damageDoneInfo, damageDoneInfoRow = new DamageDoneInfoMainRowViewModel(damageDoneInfo));
                    damageDoneInfoRows.Add(damageDoneInfoRow);
                }
                damageDoneInfoRow.Update(displayIndex++);
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
            if (_damageMeter == null) return;

            StopDamageMeterUpdater();
            ClearRows();

            _damageMeter.InitializeNewFight();
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

                if (_damageMeter == null) return;
                lock (_damageMeter)
                {
                    _damageMeter.IsPaused = IsPaused;
                }
            }
        }

        private void ClearRows()
        {
            _damageDoneRowMap.Clear();
            _damageDoneRows.Clear();
            _damageDoneInfoRowMapMap.Clear();
            _damageDoneInfoRowsMap.Clear();
            Rows?.Clear();
        }

        public void DisposeDamageMeter()
        {
            StopDamageMeterUpdater();
            _damageMeter?.Dispose();
            _damageMeter = null;
        }
    }
}
