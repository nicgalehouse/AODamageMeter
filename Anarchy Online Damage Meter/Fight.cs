using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anarchy_Online_Damage_Meter
{
    class Fight
    {
        private int _startTime = -1;
        private int _timeOfLatestAction = -1;
        private Stopwatch _duration = new Stopwatch();
        private List<Event> history = new List<Event>();
        private List<Character> charactersList = new List<Character>();

        public List<Character> CharactersList
        {
            get
            {
                return charactersList;
            }

            private set
            {
                charactersList = value;
            }
        }

        public int StartTime
        {
            get
            {
                return _startTime;
            }

            private set
            {
                _startTime = value;
            }
        }

        public int TimeOfLatestAction
        {
            get
            {
                return _timeOfLatestAction;
            }

            private set
            {
                _timeOfLatestAction = value;
            }
        }

        public Stopwatch Duration
        {
            get
            {
                return _duration;
            }

            private set
            {
                _duration = value;
            }
        }

        public Fight()
        {

        }

        public Fight(Event loggedEvent)
        {
            addEvent(loggedEvent);
        }

        public void ListCharacters()
        {
            foreach (Character character in charactersList)
            {
                Console.WriteLine(character.Name);
            }
        }

        public void addEvent(Event loggedEvent)
        {
            setTimes(loggedEvent);
            history.Add(loggedEvent);

            int indexOfSource = charactersList.FindIndex(Character => Character.Name == loggedEvent.Source);
            int indexOfTarget = charactersList.FindIndex(Character => Character.Name == loggedEvent.Target);

            if (indexOfSource != -1)
            {
                charactersList[indexOfSource].addEvent(loggedEvent, true);
            }
            else
            {
                charactersList.Add(new Character(loggedEvent, true));
            }
            if (indexOfTarget != -1)
            {
                charactersList[indexOfTarget].addEvent(loggedEvent, false);
            }
            else
            {
                charactersList.Add(new Character(loggedEvent, false));
            }

        }

        private void setTimes(Event loggedEvent)
        {
            if (!Duration.IsRunning)
            {
                Duration.Start();
            }

            if (StartTime == -1)
            {
                StartTime = loggedEvent.Timestamp;
            }

            TimeOfLatestAction = loggedEvent.Timestamp;

            if (StartTime  == TimeOfLatestAction)
            {
                TimeOfLatestAction = StartTime + 1;
            }
        }

        public void listCharacterDamage()
        {
            foreach (Character character in charactersList)
            {
                //Console.WriteLine(character.Name + ", " + character.DamageDone + ", " + (character.DamageDone / (Duration.ElapsedMilliseconds/1000)) + ", " + character.HitAttempts + ", " + StartTime + ":::" + TimeOfLatestAction);
                Console.WriteLine(character.Name + ", " + character.DamageDone + ", " + character.GetDPS(Duration.ElapsedMilliseconds) + ", " + character.HitAttempts);
            }
        }
    }
}
