using Anarchy_Online_Damage_Meter.Model;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Commands;
using Microsoft.Win32;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;
using Anarchy_Online_Damage_Meter.View;
using System.Windows;
using Anarchy_Online_Damage_Meter.Helpers;
using Anarchy_Online_Damage_Meter.Extensions;
using System.Threading;
using System.Diagnostics;

namespace Anarchy_Online_Damage_Meter.ViewModel
{
    public class MainWindowViewModel : BindableBase
    {
        public DamageMeter Meter = new DamageMeter();

        private BackgroundWorker worker;
        private string logFile;

        private ObservableCollection<DamageDoneRow> DamageDoneRows = new ObservableCollection<DamageDoneRow>();
        public ICollectionViewLiveShaping LiveCollection { get; set; }

        public ICommand ResetMeterCommand => new DelegateCommand(ResetMeter);
        public void ResetMeter()
        {
            if (logFile == null)
                return;

            worker.CancelAsync();
        }

        public ICommand FileBrowseCommand => new DelegateCommand(FileBrowse);
        public void FileBrowse()
        {
            if (worker != null)
                worker.CancelAsync();

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.FileName = "Log.txt";
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Log File(*.txt)|*.txt";

            bool? result = dialog.ShowDialog();

            if (result == true && logFile != dialog.FileName)
            {
                logFile = dialog.FileName;

                Meter = new DamageMeter(logFile);

                StartBackgroundWorker();
            }
        }

        public ICommand TerminateCommand => new DelegateCommand(Terminate);
        public void Terminate()
        {
            Application.Current.Shutdown();
        }

        public MainWindowViewModel()
        {
            IEnumerable<string> localAll = Process.GetProcessesByName("AnarchyOnline").Select(p => p.MainWindowTitle);

            LiveCollection = (ICollectionViewLiveShaping)CollectionViewSource.GetDefaultView(DamageDoneRows);
            LiveCollection.IsLiveSorting = true;

            MainWindow.View.DamageMeter.Items.SortDescriptions.Add(new SortDescription("DamageDone", ListSortDirection.Descending));
        }

        void StartBackgroundWorker()
        {
            DamageDoneRows.RemoveAll();

            worker = new BackgroundWorker();

            worker.DoWork += new DoWorkEventHandler(DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;

            worker.RunWorkerAsync();
        }

        public void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            foreach (var character in (List<Character>)e.UserState)
            {
                if (DamageDoneRows.Any(c => c.Name == character.Name))
                {
                    DamageDoneRows.Single(c => c.Name == character.Name).Update(character);
                }
                else if (character.DamageDone != 0)
                {
                    DamageDoneRows.Add(DamageDoneRow.Create(character));
                }
            }
        }

        public void DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    Meter = new DamageMeter(logFile);
                    return;
                }
                Meter.Update();
                worker.ReportProgress(0, Meter.CurrentFight.CharactersList);
                Thread.Sleep(300);
            }
        }

        public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                StartBackgroundWorker();
            }
        }
    }
}