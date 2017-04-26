using AODamageMeter.UI.Utilities;
using AODamageMeter.UI.ViewModels.Rows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                    foreach (var fightCharacter in _damageMeter.CurrentFight.FightCharacters)
                    {
                        if (_damageDoneRowsMap.TryGetValue(fightCharacter, out DamageDoneRowViewModel row))
                        {
                            row.Update();
                        }
                        else
                        {
                            _damageDoneRowsMap.Add(fightCharacter, row = new DamageDoneRowViewModel(fightCharacter));
                            DamageDoneRows.Add(row);
                        }
                    }
                }
            });

            ResetDamageMeterCommand = new RelayCommand(ExecuteResetDamageMeterCommand);
        }

        private Dictionary<FightCharacter, DamageDoneRowViewModel> _damageDoneRowsMap = new Dictionary<FightCharacter, DamageDoneRowViewModel>();
        public ObservableCollection<DamageDoneRowViewModel> DamageDoneRows { get; } = new ObservableCollection<DamageDoneRowViewModel>();

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
