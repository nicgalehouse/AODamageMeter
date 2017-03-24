using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AODamageMeter
{
    public class DamageMeter
    {
        public Fight CurrentFight = new Fight();
        private List<Fight> pastFights = new List<Fight>();
        private FileStream logFileStream;
        private StreamReader LogStreamReader;
        private string meterOwner = null;

        public DamageMeter() { }

        public DamageMeter(string filename)
        {
            FindDamageMeterOwner(filename);
            logFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            LogStreamReader = new StreamReader(logFileStream);
            LogStreamReader.ReadToEnd();
        }

        public void FindDamageMeterOwner(string filename)
        {
            DirectoryInfo characterDirectory = new DirectoryInfo(filename);
            string characterID = null; 

            try
            {
                while (characterDirectory.Parent.Exists)
                {
                    string directoryName = characterDirectory.Parent.ToString();

                    if (directoryName.Contains("Char"))
                    {
                        characterID = directoryName.Substring(4);
                    }

                    characterDirectory = characterDirectory.Parent;
                }
            }
            catch { }

            if (characterID != null)
            {
                IEnumerable<Process> processes = Process.GetProcessesByName("AnarchyOnline");
                foreach (Process p in processes)
                {
                    string anarchyOnlineTitle = "Anarchy Online - ";
                    string characterName = p.MainWindowTitle.Substring(anarchyOnlineTitle.Length);

                    CharacterBio bio = new CharacterBio(characterName);

                    if (bio.CharacterInfo.CharacterID == characterID)
                    {
                        meterOwner = bio.CharacterInfo.Name;
                    }
                }
            }

        }

        public void Update()
        {
            string line;
            while ((line = LogStreamReader.ReadLine()) != null)
            {
                CurrentFight.AddEvent(new Event(line, meterOwner));
            }
            CurrentFight.UpdateCharactersTime();
        }
    }
}
