using System.ComponentModel;
using Anarchy_Online_Damage_Meter.ViewModel;

namespace Anarchy_Online_Damage_Meter
{
    public class Meter
    {
        private MainWindowViewModel viewModel;
        private BackgroundWorker worker;

        public Meter(MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
        }


        //        public Fight CurrentFight = new Fight();
        //        private List<Fight> pastFights = new List<Fight>();
        //        private FileStream logFileStream;
        //        public StreamReader LogStreamReader;

        //        public DamageMeter2() { }

        //        public DamageMeter2(string filename)
        //        {
        //            logFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //            LogStreamReader = new StreamReader(logFileStream);
        //            LogStreamReader.ReadToEnd();
        //        }

        //        public void Update()
        //        {
        //            string line;
        //            while ((line = LogStreamReader.ReadLine()) != null)
        //            {
        //                CurrentFight.AddEvent(new Event(line));
        //            }
        //            CurrentFight.UpdateCharactersTime();
        //        }

        //        void Start()
        //        {
        //            DamageDoneRows.RemoveAll();

        //            worker = new BackgroundWorker();

        //            worker.DoWork += new DoWorkEventHandler(DoWork);
        //            worker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
        //            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
        //            worker.WorkerReportsProgress = true;
        //            worker.WorkerSupportsCancellation = true;

        //            worker.RunWorkerAsync();
        //        }

        //        public void ProgressChanged(object sender, ProgressChangedEventArgs e)
        //        {
        //            foreach (var character in (List<Character>)e.UserState)
        //            {
        //                if (DamageDoneRows.Any(c => c.Name == character.Name))
        //                {
        //                    DamageDoneRows.Single(c => c.Name == character.Name).Update(character);
        //                }
        //                else if (character.DamageDone != 0)
        //                {
        //                    DamageDoneRows.Add(DamageDoneRow.Create(character));
        //                }
        //            }
        //        }

        //        public void DoWork(object sender, DoWorkEventArgs e)
        //        {
        //            while (true)
        //            {
        //                if (worker.CancellationPending)
        //                {
        //                    e.Cancel = true;
        //                    Meter = new DamageMeter(logFile);
        //                    return;
        //                }
        //                Meter.Update();
        //                worker.ReportProgress(0, Meter.CurrentFight.CharactersList);
        //                Thread.Sleep(300);
        //            }
        //        }

        //        public void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        //        {
        //            if (e.Cancelled)
        //            {
        //                Start();
        //            }
        //        }
    }
}
