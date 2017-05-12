using AODamageMeter.UI.Properties;
using AODamageMeter.UI.Utilities;
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
        private IProgress<object> _damageMeterProgressReporter;
        private CancellationTokenSource _damageMeterUpdaterCTS;
        private Task _damageMeterUpdater;
        private bool _isDamageMeterUpdaterStarted;

        public DamageMeterViewModel()
        {
            _damageMeterProgressReporter = new Progress<object>(_ =>
            {
                if (_damageMeter == null) return; // Edge case where reporter lags behind cancellation/disposal.
                lock (_damageMeter)
                {
                    int displayIndex = 1;
                    foreach (var fightCharacter in _damageMeter.CurrentFight.FightCharacters
                        .OrderByDescending(c => c.DamageDonePlusPets)
                        .ThenBy(c => c.Name))
                    {
                         // A fight character may not be immediately recognized as a pet; remove it if it becomes one.
                        if (fightCharacter.IsPet)
                        {
                            if (_damageDoneRowViewModelsMap.TryGetValue(fightCharacter, out DamageDoneMainRowViewModel damageDoneRow))
                            {
                                _damageDoneRowViewModelsMap.Remove(fightCharacter);
                                DamageDoneRowViewModels.Remove(damageDoneRow);
                            }
                        }
                        else
                        {
                            if (!_damageDoneRowViewModelsMap.TryGetValue(fightCharacter, out DamageDoneMainRowViewModel damageDoneRowViewModel))
                            {
                                _damageDoneRowViewModelsMap.Add(fightCharacter, damageDoneRowViewModel = new DamageDoneMainRowViewModel(fightCharacter));
                                DamageDoneRowViewModels.Add(damageDoneRowViewModel);
                            }
                            damageDoneRowViewModel.Update(displayIndex++);
                        }
                    }
                }
            });

            ResetDamageMeterCommand = new RelayCommand(ExecuteResetDamageMeterCommand);
            ToggleIsPausedCommand = new RelayCommand(ExecuteToggleIsPausedCommand);
            TryInitializeDamageMeter(Settings.Default.SelectedCharacterName, Settings.Default.SelectedLogFilePath);
        }

        private Dictionary<FightCharacter, DamageDoneMainRowViewModel> _damageDoneRowViewModelsMap = new Dictionary<FightCharacter, DamageDoneMainRowViewModel>();
        public ObservableCollection<DamageDoneMainRowViewModel> DamageDoneRowViewModels { get; } = new ObservableCollection<DamageDoneMainRowViewModel>();

        public bool TryInitializeDamageMeter(string characterName, string logFilePath)
        {
            // No reason to reinitialize if same name/path AND we succeeded before (AKA _damageMeter not null).
            if (_characterName == characterName && _logFilePath == logFilePath && _damageMeter != null)
                return true;

            _characterName = characterName;
            _logFilePath = logFilePath;

            DisposeDamageMeter();
            _damageDoneRowViewModelsMap.Clear();
            DamageDoneRowViewModels.Clear();

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
                    _damageMeterProgressReporter.Report(null);
                } while (!_damageMeterUpdaterCTS.Token.WaitHandle.WaitOne(300));
            }, _damageMeterUpdaterCTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _isDamageMeterUpdaterStarted = true;
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
            _damageDoneRowViewModelsMap.Clear();
            DamageDoneRowViewModels.Clear();

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

        public void DisposeDamageMeter()
        {
            StopDamageMeterUpdater();
            _damageMeter?.Dispose();
            _damageMeter = null;
        }
    }
}
