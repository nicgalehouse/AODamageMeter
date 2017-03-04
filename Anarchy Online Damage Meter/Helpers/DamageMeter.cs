using System.IO;
using System.Collections.Generic;
using Anarchy_Online_Damage_Meter.Helpers;

namespace Anarchy_Online_Damage_Meter
{
    public class DamageMeter
    {
        public Fight CurrentFight = new Fight();
        private List<Fight> pastFights = new List<Fight>();
        private FileStream logFileStream;
        public StreamReader LogStreamReader;

        public DamageMeter() { }

        public DamageMeter(string filename)
        {
            logFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            LogStreamReader = new StreamReader(logFileStream);
            LogStreamReader.ReadToEnd();
        }

        public void Update()
        {
            string line;
            while ((line = LogStreamReader.ReadLine()) != null)
            {
                CurrentFight.AddEvent(new Event(line));
            }
            CurrentFight.UpdateCharactersTime();
        }
    }
}
