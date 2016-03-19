using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anarchy_Online_Damage_Meter
{
    class LogFileReader
    {
        private static string filename;
        private FileStream logFileStream;
        

        public LogFileReader(string filename)
        {
            //filename = @"C:\Users\nicga\AppData\Local\Funcom\Anarchy Online\70dad3e6\Anarchy Online\Prefs\eugenedavid\Char484883214\Chat\Windows\Window4\Log.txt";
            logFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader logStreamRead = new StreamReader(logFileStream);
            //logStreamRead.BaseStream.Seek(0,SeekOrigin.End);

            String line;

            int lineNumber = 1;
            while (true)
            {
                //System.Threading.WaitHandle.WaitTimeout;
                line = logStreamRead.ReadLine();
                if (line != null)
                {
                    //parse log START
                    Event parsedLine = new Event(line);

                    Console.WriteLine("*****************************LINE NUMBER " + lineNumber + " **************************");
                    Console.WriteLine(line);
                    Console.WriteLine("Key: " + parsedLine.GetKey());
                    Console.WriteLine("Event Time: " + parsedLine.GetTimeStamp());
                    Console.WriteLine("Event Amount: " + parsedLine.GetAmount());
                    Console.WriteLine("Event Amount Type: " + parsedLine.GetAmountType());
                    Console.WriteLine("Event Target: " + parsedLine.GetTarget());
                    Console.WriteLine("Event Action: " + parsedLine.GetAction());
                    Console.WriteLine("Event Source: " + parsedLine.GetSource());
                    Console.WriteLine("Event Modifier: " + parsedLine.GetModifier());

                    lineNumber++;

                    //parse log END
                    
                }
                //System.Threading.Thread.Sleep(1500);
            }
        }

        private void readFile(FileStream logFileStream)
        {
            logFileStream.Seek(5, SeekOrigin.End);
        }
    }
}
