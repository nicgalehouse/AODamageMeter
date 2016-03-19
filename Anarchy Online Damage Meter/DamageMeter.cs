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
        Fight overall;
        Fight currentFight;
        List<Fight> pastFights;

        public DamageMeter(string filename)
        {
            FileStream logFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader logStreamRead = new StreamReader(logFileStream);
        }


        public void clearAll()
        {

        }

        public void report()
        {

        }

        public void removeFight()
        {

        }
    }
}
