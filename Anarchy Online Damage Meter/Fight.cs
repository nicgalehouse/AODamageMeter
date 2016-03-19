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
        private List<Character> charactersList = new List<Character>();

        public Fight()
        {

        }

        public Fight(Event loggedEvent)
        {
            addEvent(loggedEvent);
        }

        public void listCharacters()
        {
            foreach(Character character in charactersList)
            {
                Console.WriteLine(character.getName());
            }
        }

        public void addEvent(Event loggedEvent)
        {

            setTimes(loggedEvent);

            int index = charactersList.FindIndex(Character => Character.getName() == loggedEvent.GetSource());

            if (index != -1)
            {
                charactersList[index].addEvent(loggedEvent);
            }
            else
            {
                charactersList.Add(new Character(loggedEvent));
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
            return charactersList.Any(Character => Character.getName() == loggedEvent.GetSource());
        }

    }
}
