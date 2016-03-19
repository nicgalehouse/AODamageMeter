using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anarchy_Online_Damage_Meter
{
    class Fight
    {
        private int startTime = -1;
        private int endTime = -1;
        private int totalDamage;
        private List<Character> characters;

        public Fight(Event loggedEvent)
        {

        }

        public void addEvent(Event loggedEvent)
        {

            setTimes(loggedEvent);

            int index = characters.FindIndex(Character => Character.getName() == loggedEvent.GetSource());

            if (index != -1)
            {
                characters[index].addEvent(loggedEvent);
            }
            else
            {
                characters.Add(new Character(loggedEvent));
            }   

        }

        private void setTimes(Event loggedEvent)
        {
            if (startTime == -1)
            {
                startTime = loggedEvent.GetTimeStamp();
            }

            endTime = loggedEvent.GetTimeStamp();
        }

        private bool characterExists(Event loggedEvent)
        {
            return characters.Any(Character => Character.getName() == loggedEvent.GetSource());
        }

    }
}
