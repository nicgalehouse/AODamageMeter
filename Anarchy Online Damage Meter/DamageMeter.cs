using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anarchy_Online_Damage_Meter
{
    class DamageMeter
    {
        private Fight overallFight = new Fight();
        private Fight currentFight = new Fight();
        private List<Fight> pastFights = new List<Fight>();
        private FileStream logFileStream;
        private StreamReader logStreamRead;


        public Fight CurrentFight
        {
            get
            {
                return overallFight;
            }

            private set
            {
                overallFight = value;
            }
        }

        public DamageMeter(string filename)
        {

            
            logFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            logStreamRead = new StreamReader(logFileStream);
            /*
            //while (!logStreamRead.EndOfStream)
            while (true)
            {
                //System.Threading.WaitHandle.WaitTimeout;
                line = logStreamRead.ReadLine();
                if (line != null)
                {
                    Event parsedLine = new Event(line);
                    overallFight.addEvent(new Event(line));
                }

                //overallFight.listCharacterDamage();
                //System.Threading.Thread.Sleep(1500);
                
            }
            */

        }

        public void readFile()
        {
            String line;
           while (!logStreamRead.EndOfStream)
            //while (true)
            {
                //System.Threading.WaitHandle.WaitTimeout;
                line = logStreamRead.ReadLine();
                if (line != null)
                {
                    Event parsedLine = new Event(line);
                    overallFight.addEvent(new Event(line));
                }

                //overallFight.listCharacterDamage();
                //System.Threading.Thread.Sleep(1500);

            }
        }

        public void clearAll()
        {

        }

        public void listOverallCharacters()
        {
            overallFight.listCharacterDamage();
        }

        public void report()
        {

        }

        public void removeFight()
        {

        }
    }
}
