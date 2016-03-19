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

        private List<Event> events = new List<Event>();

        public Character(Event loggedEvent)
        {
            name = loggedEvent.GetSource();
            events.Add(loggedEvent);
        }

        public string getName()
        {
            return name;
        }

        public void addEvent(Event loggedEvent)
        {
            events.Add(loggedEvent);
        }

    }
}
