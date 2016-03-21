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
        private string _name;
        private int _hitAttempts;
        private int _missAmount;
        private int _attacksReceived;
        private int _dodgeAmount;
        private int _damageDone;
        private int _damageTaken;
        private int _numberOfCrits;
        private int _healingDone;
        private int _healingReceived;
        private int _absorbAmount;

        private List<Event> _damageTakenEvents = new List<Event>();
        private List<Event> _damageDoneEvents = new List<Event>();
        private List<Event> _healingDoneEvents = new List<Event>();
        private List<Event> _healingReceivedEvents = new List<Event>();
        private List<Event> _absorbEvents = new List<Event>();

        public int CritChance
        {
            get
            {
                return (NumberOfCrits/HitAttempts)*100;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            private set
            {
                _name = value;
            }
        }

        public int DamageDone
        {
            get
            {
                return _damageDone;
            }

            private set
            {
                _damageDone = value;
            }
        }

        public int DamageTaken
        {
            get
            {
                return _damageTaken;
            }

            private set
            {
                _damageTaken = value;
            }
        }

        public int NumberOfCrits
        {
            get
            {
                return _numberOfCrits;
            }

            private set
            {
                _numberOfCrits = value;
            }
        }

        public int MissChance
        {
            get
            {
                return (MissAmount/HitAttempts)*100;
            }
        }

        public int HealingDone
        {
            get
            {
                return _healingDone;
            }

            private set
            {
                _healingDone = value;
            }
        }

        public int HealingReceived
        {
            get
            {
                return _healingReceived;
            }

            private set
            {
                _healingReceived = value;
            }
        }

        public int AbsorbAmount
        {
            get
            {
                return _absorbAmount;
            }

            private set
            {
                _absorbAmount = value;
            }
        }

        public int MissAmount
        {
            get
            {
                return _missAmount;
            }

            set
            {
                _missAmount = value;
            }
        }

        public int HitAttempts
        {
            get
            {
                return _hitAttempts;
            }

            private set
            {
                _hitAttempts = value;
            }
        }

        public int GetDPS(long elapsedTimeInMilliseconds)
        {
            
            if (unchecked((int)elapsedTimeInMilliseconds) < 1000)
            {
                return DamageDone;
            }
            else
            { 
            return DamageDone / (unchecked((int)elapsedTimeInMilliseconds) / 1000);
            }
        }

        public Character(Event loggedEvent, bool isSource)
        {
            if (isSource)
            {
                Name = loggedEvent.Source;
                addEvent(loggedEvent, true);
            }
            else
            {
                Name = loggedEvent.Target;
                addEvent(loggedEvent, false);
            }
        }

        public void addEvent(Event loggedEvent, bool isSource)
        {
            if (isSource)
            {
                if (loggedEvent.Action == "Damage" || loggedEvent.Action == "Nano" || loggedEvent.Action == "Miss")
                {
                    _damageDoneEvents.Add(loggedEvent);
                    HitAttempts++;
                    DamageDone += loggedEvent.Amount;

                    if (loggedEvent.Modifier == "Crit")
                    {
                        NumberOfCrits++;
                    }
                    if (loggedEvent.Modifier == "Glance")
                    {
                        //something?
                    }

                    if (loggedEvent.Action == "Miss")
                    {
                        MissAmount++;
                    }

                }
                else if (loggedEvent.Action == "Heal")
                {
                    _healingDoneEvents.Add(loggedEvent);
                }
                else
                {
                    _absorbEvents.Add(loggedEvent);
                    AbsorbAmount += loggedEvent.Amount;
                }
            }
            else
            {
                if (loggedEvent.Action == "Damage" || loggedEvent.Action == "Nano" || loggedEvent.Action == "Miss")
                {
                    _damageTakenEvents.Add(loggedEvent);
                    DamageTaken += loggedEvent.Amount;
                }
                else if (loggedEvent.Action == "Heal")
                {
                    _healingReceivedEvents.Add(loggedEvent);
                    HealingReceived += loggedEvent.Amount;
                }
            }

        }

    }
}
