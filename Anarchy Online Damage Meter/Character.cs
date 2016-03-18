using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Anarchy_Online_Damage_Meter
{
    class Character
    {
        private string name;



        private List<Event> events;

        public Character(Event loggedEvent)
        {
            events.Add(loggedEvent);
        }

        public string getName()
        {
            return name;
        }


    }
}
