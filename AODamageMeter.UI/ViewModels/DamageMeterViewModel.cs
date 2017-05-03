using AODamageMeter.UI.Utilities;
using AODamageMeter.UI.ViewModels.Rows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AODamageMeter.UI.ViewModels
{
    public class DamageMeterViewModel : ViewModelBase
    {
        private DamageMeter _damageMeter;
        private IProgress<object> _damageMeterProgressReporter;
        private CancellationTokenSource _damageMeterUpdaterCTS;
        private Task _damageMeterUpdater;
        private bool _isDamageMeterUpdaterStarted;

        public DamageMeterViewModel()
        {
            _damageMeterProgressReporter = new Progress<object>(_ =>
            {
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
                            if (_damageDoneRowsMap.TryGetValue(fightCharacter, out DamageDoneMainRowViewModel damageDoneRow))
                            {
                                _damageDoneRowsMap.Remove(fightCharacter);
                                DamageDoneRows.Remove(damageDoneRow);
                            }
                        }
                        else
                        {
                            if (!_damageDoneRowsMap.TryGetValue(fightCharacter, out DamageDoneMainRowViewModel damageDoneRow))
                            {
                                _damageDoneRowsMap.Add(fightCharacter, damageDoneRow = new DamageDoneMainRowViewModel(fightCharacter));
                                DamageDoneRows.Add(damageDoneRow);
                            }
                            damageDoneRow.Update(displayIndex++);
                        }
                    }
                }
            });

            ResetDamageMeterCommand = new RelayCommand(ExecuteResetDamageMeterCommand);
        }

        private Dictionary<FightCharacter, DamageDoneMainRowViewModel> _damageDoneRowsMap = new Dictionary<FightCharacter, DamageDoneMainRowViewModel>();
        public ObservableCollection<DamageDoneMainRowViewModel> DamageDoneRows { get; } = new ObservableCollection<DamageDoneMainRowViewModel>();

        public void SetLogFile(string logFilePath)
        {
            DisposeDamageMeter();
            _damageDoneRowsMap.Clear();
            DamageDoneRows.Clear();

            _damageMeter = new DamageMeter(logFilePath);
#if DEBUG
            _damageMeter.InitializeNewFight(skipToEndOfLog: false);
#else
            _damageMeter.InitializeNewFight();
#endif

            StartDamageMeterUpdater();
        }

        public ICommand ResetDamageMeterCommand { get; }
        public void ExecuteResetDamageMeterCommand()
        {
            if (_damageMeter == null) return;

            StopDamageMeterUpdater();
            _damageDoneRowsMap.Clear();
            DamageDoneRows.Clear();

            _damageMeter.InitializeNewFight();
            StartDamageMeterUpdater();
        }

        protected void StartDamageMeterUpdater()
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
                    _damageMeterProgressReporter.Report(null);
                } while (!_damageMeterUpdaterCTS.Token.WaitHandle.WaitOne(300));
            }, _damageMeterUpdaterCTS.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            _isDamageMeterUpdaterStarted = true;
        }

        protected void StopDamageMeterUpdater()
        {
            if (!_isDamageMeterUpdaterStarted) return;

            _isDamageMeterUpdaterStarted = false;
            _damageMeterUpdaterCTS.Cancel();
            _damageMeterUpdater.Wait();
            _damageMeterUpdaterCTS.Dispose();
        }

        public void DisposeDamageMeter()
        {
            StopDamageMeterUpdater();
            _damageMeter?.Dispose();
        }
    }
}
