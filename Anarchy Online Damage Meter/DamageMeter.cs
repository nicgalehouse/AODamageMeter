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

        public DamageMeter(string filename)
        {
            FileStream logFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader logStreamRead = new StreamReader(logFileStream);
            String line;
            int lineNumber = 1;
            while (!logStreamRead.EndOfStream)
            {
                //System.Threading.WaitHandle.WaitTimeout;
                line = logStreamRead.ReadLine();
                if (line != null)
                {
                    //parse log START
                    Event parsedLine = new Event(line);
                    overallFight.addEvent(new Event(line));

                    
                    Console.WriteLine(lineNumber);
                    /*
                    Console.WriteLine(line);
                    Console.WriteLine("Key: " + parsedLine.GetKey());
                    Console.WriteLine("Event Time: " + parsedLine.GetTimeStamp());
                    Console.WriteLine("Event Amount: " + parsedLine.GetAmount());
                    Console.WriteLine("Event Amount Type: " + parsedLine.GetAmountType());
                    Console.WriteLine("Event Target: " + parsedLine.GetTarget());
                    Console.WriteLine("Event Action: " + parsedLine.GetAction());
                    Console.WriteLine("Event Source: " + parsedLine.GetSource());
                    Console.WriteLine("Event Modifier: " + parsedLine.GetModifier());
                    */
                    
                    lineNumber++;

                    //parse log END

                }
                //System.Threading.Thread.Sleep(1500);

            }

        }

        public void clearAll()
        {

        }

        public void listOverallCharacters()
        {
            overallFight.listCharacters();
        }

        public void report()
        {

        }

        public void removeFight()
        {

        }
    }
}
