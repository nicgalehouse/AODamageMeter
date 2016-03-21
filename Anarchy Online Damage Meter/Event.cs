using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anarchy_Online_Damage_Meter
{
    class Event
    {
        
        private string _key = "";
        private int _timestamp = 0;
        //Damage, Nano, Heal, Absorb, PetDamage, Miss
        private string _action = "";
        private string _source = "Unknown-Source";
        private string _target = "Unknown-Source";
        private int _amount = 0;
        private string _amountType = "";
        //Crit, Glance
        private string _modifier = "";

        public string Key
        {
            get
            {
                return _key;
            }

            private set
            {
                _key = value;
            }
        }

        public int Timestamp
        {
            get
            {
                return _timestamp;
            }

            private set
            {
                _timestamp = value;
            }
        }

        public string Action
        {
            get
            {
                return _action;
            }

            private set
            {
                _action = value;
            }
        }

        public string Source
        {
            get
            {
                return _source;
            }

            private set
            {
                _source = value;
            }
        }

        public string Target
        {
            get
            {
                return _target;
            }

            private set
            {
                _target = value;
            }
        }

        public int Amount
        {
            get
            {
                return _amount;
            }

            private set
            {
                _amount = value;
            }
        }

        public string AmountType
        {
            get
            {
                return _amountType;
            }

            private set
            {
                _amountType = value;
            }
        }

        public string Modifier
        {
            get
            {
                return _modifier;
            }

            private set
            {
                _modifier = value;
            }
        }

        public Event(string line)
        {
            Key = line.Substring(17, 2);
            SetTime(Key, line);
            SetAttributes(Key, ParseMessage(Key, line));
        }

        private void SetKey(string line)
        {
            //EXAMPLE LINE:
            //["#000000004200000a#","Other hit by other","",1457816347]SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage.
            //Digits indexed at 17 and 18, "0a" in this example. 
            Key = line.Substring(17, 2);
        }

        private void SetTime(string key, string line)
        {
            int timeLength = 10;
            switch (key)
            {
                case "0a":
                    Timestamp = Convert.ToInt32(line.Substring(46, timeLength));
                    break;
                case "05":
                    Timestamp = Convert.ToInt32(line.Substring(51, timeLength));
                    break;
                case "04":
                    Timestamp = Convert.ToInt32(line.Substring(45, timeLength));
                    break;
                case "15":
                    Timestamp = Convert.ToInt32(line.Substring(41, timeLength));
                    break;
                case "08":
                    Timestamp = Convert.ToInt32(line.Substring(41, timeLength));
                    break;
                case "06":
                    Timestamp = Convert.ToInt32(line.Substring(45, timeLength));
                    break;
                case "13":
                    Timestamp = Convert.ToInt32(line.Substring(40, timeLength));
                    break;
                case "16":
                    Timestamp = Convert.ToInt32(line.Substring(39, timeLength));
                    break;
                case "12":
                    Timestamp = Convert.ToInt32(line.Substring(39, timeLength));
                    break;
                case "17":
                    Timestamp = Convert.ToInt32(line.Substring(41, timeLength));
                    break;
                case "09":
                    Timestamp = Convert.ToInt32(line.Substring(49, timeLength));
                    break;
                case "0b":
                    Timestamp = Convert.ToInt32(line.Substring(37, timeLength));
                    break;
                case "18":
                    Timestamp = Convert.ToInt32(line.Substring(40, timeLength));
                    break;
            }
        }

        private string ParseMessage(string key, string line)
        {
            switch (key)
            {
                case "0a":
                    return line.Substring(57, line.Length - 57);
                case "05":
                    return line.Substring(62, line.Length - 62);
                case "04":
                    return line.Substring(56, line.Length - 56);
                case "15":
                    return line.Substring(52, line.Length - 52);
                case "08":
                    return line.Substring(52, line.Length - 52);
                case "06":
                    return line.Substring(56, line.Length - 56);
                case "13":
                    return line.Substring(51, line.Length - 51);
                case "16":
                    return line.Substring(50, line.Length - 50);
                case "12":
                    return line.Substring(50, line.Length - 50);
                case "17":
                    return line.Substring(52, line.Length - 52);
                case "09":
                    return line.Substring(60, line.Length - 60);
                case "0b":
                    return line.Substring(48, line.Length - 48);
                case "18":
                    return line.Substring(51, line.Length - 51);
            }
            return "";
        }

        private void SetAttributes(string key, string line)
        {
            int indexOfSource, lengthOfSource;
            int indexOfTarget, lengthOfTarget;
            int indexOfAmount, lengthOfAmount;
            int indexOfAmountType, lengthOfAmountType;

            switch (key)
            {
                //Every "you", "me" == SOURCE
                //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage.
                //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                //SOURCE's reflect shield hit TARGET for AMOUNT points of damage.
                //SOURCE's damage shield hit TARGET for AMOUNT points of damage.
                //Something hit TARGET for AMOUNT points of damage by damage shield.
                //Something hit TARGET for AMOUNT points of damage by reflect shield.
                //Someone absorbed AMOUNT points of AMOUNTTYPE damage.
                case "0a":

                    //We don't have enough info about this event to do anything.
                    //Someone absorbed AMOUNT points of AMOUNTTYPE damage.
                    if (line.LastIndexOf("Someone absorbed ") != -1)
                    {
                        break;
                    }
                    //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage.
                    if (line.IndexOf(" shield hit ") == -1 && line.IndexOf("shield.") == -1)
                    {
                        indexOfSource = 0;
                        lengthOfSource = line.IndexOf(" hit ") - indexOfSource;
                        // + 5 to skip the " hit " indices
                        indexOfTarget = indexOfSource + lengthOfSource + 5;
                        lengthOfTarget = line.IndexOf(" for ") - indexOfTarget;

                        indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                        lengthOfAmountType = line.IndexOf(" damage.") - indexOfAmountType;

                        AmountType = line.Substring(indexOfAmountType, lengthOfAmountType);
                        Source = line.Substring(indexOfSource, lengthOfSource);

                        //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                        if (line[line.Length - 1] == '!')
                        {
                            Modifier = "Crit";
                        }
                        //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                        else if (line[line.Length - 2] == 't')
                        {
                            Modifier = "Glance";
                        }
                    }
                    //SOURCE's reflect shield hit TARGET for AMOUNT points of damage.
                    //SOURCE's damage shield hit TARGET for AMOUNT points of damage.
                    else if (line.IndexOf(" shield hit ") != -1)
                    {
                        indexOfSource = 0;
                        //SOURCE's reflect shield hit TARGET for AMOUNT points of damage.
                        if (line.IndexOf(" reflect ") != -1)
                        {
                            lengthOfSource = line.IndexOf(" reflect shield ") - 2 - indexOfSource;
                            indexOfTarget = indexOfSource + lengthOfSource + 22;
                            AmountType = "Reflect";
                        }
                        //SOURCE's damage shield hit TARGET for AMOUNT points of damage.
                        else
                        {
                            lengthOfSource = line.IndexOf(" damage shield ") - 2 - indexOfSource;
                            indexOfTarget = indexOfSource + lengthOfSource + 21;
                            AmountType = "Shield";
                        }

                        lengthOfTarget = line.IndexOf(" for ") - indexOfTarget;

                        indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        Source = line.Substring(indexOfSource, lengthOfSource);
                    }
                    //Something hit TARGET for AMOUNT points of damage by damage shield.
                    //Something hit TARGET for AMOUNT points of damage by reflect shield.
                    else
                    {
                        indexOfTarget = 14;
                        lengthOfTarget = line.IndexOf(" for ") - indexOfTarget;

                        indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        //Something hit TARGET for AMOUNT points of damage by damage shield.
                        if (line[line.Length - 9] == 'e')
                        {
                            AmountType = "Shield";
                        }
                        //Something hit TARGET for AMOUNT points of damage by reflect shield.
                        else
                        {
                            AmountType = "Reflect";
                        }

                    }

                    Target = line.Substring(indexOfTarget, lengthOfTarget);
                    Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                    Action = "Damage";

                    break;

                //You hit TARGET with nanobots for AMOUNT points of AMOUNTTYPE damage.
                case "05":
                    {
                        indexOfTarget = 8;
                        lengthOfTarget = line.IndexOf(" with ") - indexOfTarget;

                        indexOfAmount = indexOfTarget + lengthOfTarget + 19;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                        lengthOfAmountType = line.Length - 8 - indexOfAmountType;

                        Action = "Damage";
                        Source = "userOfTheDamageMeter";
                        Target = line.Substring(indexOfTarget, lengthOfTarget);
                        Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                        AmountType = line.Substring(indexOfAmountType, lengthOfAmountType);

                        //You hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                        if (line[line.Length - 1] == '!')
                        {
                            Modifier = "Crit";
                        }
                        //You hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                        else if (line[line.Length - 2] == 't')
                        {
                            Modifier = "Glance";
                        }
                    }

                    break;

                //TARGET was attacked with nanobots from SOURCE for AMOUNT points of AMOUNTTYPE damage.
                //TARGET was attacked with nanobots for AMOUNT points of AMOUNTTYPE damage.
                case "04":

                    indexOfTarget = 0;
                    lengthOfTarget = line.IndexOf(" was ");

                    //TARGET was attacked with nanobots from SOURCE for AMOUNT points of AMOUNTTYPE damage.
                    if (line.IndexOf(" from ") != -1)
                    {
                        indexOfSource = indexOfTarget + lengthOfTarget + 33;
                        lengthOfSource = line.IndexOf(" for ") - indexOfSource;
                        indexOfAmount = indexOfSource + lengthOfSource + 5;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        Source = line.Substring(indexOfSource, lengthOfSource);

                    }
                    //TARGET was attacked with nanobots for AMOUNT points of AMOUNTTYPE damage.
                    else
                    {
                        indexOfAmount = indexOfTarget + lengthOfTarget + 32;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        //might not be true
                        Source = "Unknown-Source";

                    }

                    indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                    lengthOfAmountType = line.Length - 8 - indexOfAmountType;

                    Action = "Nano";
                    Target = line.Substring(indexOfTarget, lengthOfTarget);
                    Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = line.Substring(indexOfAmountType, lengthOfAmountType);

                    break;

                //You were healed for AMOUNT points.
                //You got healed by SOURCE for AMOUNT points of health.
                case "15":

                    //You were healed for AMOUNT points.
                    if (line[4] == 'w')
                    {
                        indexOfAmount = 20;
                        lengthOfAmount = line.LastIndexOf(" ") - indexOfAmount;

                        Source = "userOfTheDamageMeter";
                        Target = "userOfTheDamageMeter";

                    }
                    //You got healed by SOURCE for AMOUNT points of health.
                    else
                    {
                        indexOfSource = 18;
                        lengthOfSource = line.LastIndexOf(" for ") - indexOfSource;

                        indexOfAmount = indexOfSource + lengthOfSource + 5;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        Source = line.Substring(indexOfSource, lengthOfSource);
                        Target = "userOfTheDamageMeter";

                    }

                    Action = "Heal";
                    Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = "Heal";

                    break;

                //You hit TARGET for AMOUNT points of AMOUNTTYPE damage.
                //Your damage shield hit TARGET for AMOUNT points of damage.
                case "08":

                    if (line.StartsWith("Your"))
                    {
                        indexOfTarget = 23;
                        lengthOfTarget = line.IndexOf(" for ") - indexOfTarget;

                        indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        Action = "Damage";
                        Source = "userOfTheDamageMeter";
                        Target = line.Substring(indexOfTarget, lengthOfTarget);
                        Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                        AmountType = "Shield";
                    }
                    else
                    {
                        indexOfTarget = 8;
                        lengthOfTarget = line.IndexOf(" for ") - indexOfTarget;

                        indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                        lengthOfAmountType = line.Length - 8 - indexOfAmountType;

                        Action = "Damage";
                        Source = "userOfTheDamageMeter";
                        Target = line.Substring(indexOfTarget, lengthOfTarget);
                        Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                        AmountType = line.Substring(indexOfAmountType, lengthOfAmountType);

                        //You hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                        if (line[line.Length - 1] == '!')
                        {
                            Modifier = "Crit";
                        }
                        //You hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                        else if (line[line.Length - 2] == 't')
                        {
                            Modifier = "Glance";
                        }
                    }

                    break;

                //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage.
                //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                //Someone's reflect shield hit you for AMOUNT points of damage.
                //Someone's damage shield hit you for AMOUNT points of damage.
                //You absorbed AMOUNT points of AMOUNTTYPE damage.
                case "06":

                    //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage.
                    if (!line.StartsWith("You absorbed ") && !line.StartsWith("Someone's"))
                    {

                        indexOfSource = 0;
                        lengthOfSource = line.IndexOf(" hit ") - indexOfSource;

                        indexOfAmount = indexOfSource + lengthOfSource + 13;
                        lengthOfAmount = line.LastIndexOf(" points ") - indexOfAmount;

                        indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                        lengthOfAmountType = line.Length - 8 - indexOfAmountType;

                        Action = "Damage";
                        Source = line.Substring(indexOfSource, lengthOfSource);

                        //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                        if (line[line.Length - 1] == '!')
                        {
                            lengthOfAmountType = line.Length - 22 - indexOfAmountType;
                            Modifier = "Crit";
                        }
                        //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                        else if (line[line.Length - 2] == 't')
                        {
                            lengthOfAmountType = line.Length - 22 - indexOfAmountType;
                            Modifier = "Glance";
                        }
                        AmountType = line.Substring(indexOfAmountType, lengthOfAmountType);
                    }
                    //You absorbed AMOUNT points of AMOUNTTYPE damage.
                    else if (!line.StartsWith("Someone's"))
                    {

                        indexOfAmount = 13;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        indexOfAmountType = indexOfAmount + 8;
                        lengthOfAmountType = line.Length - 8 - indexOfAmountType;

                        Action = "Absorb";
                        Source = "userOfTheDamageMeter";
                        AmountType = line.Substring(indexOfAmountType, lengthOfAmountType);
                    }
                    //Someone's reflect shield hit you for AMOUNT points of damage.
                    //Someone's damage shield hit you for AMOUNT points of damage.
                    else
                    {
                        //Someone's damage shield hit you for AMOUNT points of damage.
                        if (line[10] == 'd')
                        {
                            indexOfAmount = 36;
                            lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;
                            AmountType = "Shield";
                        }
                        //Someone's reflect shield hit you for AMOUNT points of damage.
                        else
                        {
                            indexOfAmount = 37;
                            lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;
                            AmountType = "Reflect";
                        }
                    }

                    Target = "userOfTheDamageMeter";
                    Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));

                    break;

                //SOURCE tried to hit you, but missed!
                //SOURCE tries to attack you with Brawl, but misses!
                //SOURCE tries to attack you with FastAttack, but misses!
                //SOURCE tries to attack you with FlingShot, but misses!
                case "13":

                    indexOfSource = 0;

                    if (line.LastIndexOf("you,") == -1)
                    {

                        lengthOfSource = line.IndexOf(" tries ") - indexOfSource;
                        //should be changed in future
                        AmountType = "SpecialAttack";

                    }
                    else
                    {
                        lengthOfSource = line.IndexOf(" tried ") - indexOfSource;
                        AmountType = "Auto Attack";
                    }

                    Action = "Miss";
                    Source = line.Substring(indexOfSource, lengthOfSource);
                    Target = "userOfTheDamageMeter";

                    break;

                //You got nano from SOURCE for AMOUNT points.
                case "16":

                    indexOfSource = 18;
                    lengthOfSource = line.IndexOf(" for ") - indexOfSource;

                    indexOfAmount = indexOfSource + lengthOfSource + 5;
                    lengthOfAmount = line.Length - 8 - indexOfAmount;

                    Action = "Heal";
                    Source = line.Substring(indexOfSource, lengthOfSource);
                    Target = "userOfTheDamageMeter";
                    Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = "Nano";

                    break;

                //You tried to hit TARGET, but missed!
                case "12":


                    indexOfTarget = 17;
                    lengthOfTarget = line.Length - 13 - indexOfTarget;

                    Action = "Miss";
                    Source = "userOfTheDamageMeter";
                    Target = line.Substring(indexOfTarget, lengthOfTarget);

                    break;

                //You increased nano on TARGET for AMOUNT points.
                case "17":

                    indexOfTarget = 22;
                    lengthOfTarget = line.LastIndexOf(" for ") - indexOfTarget;

                    indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                    lengthOfAmount = line.Length - 8 - indexOfAmount;

                    Action = "Heal";
                    Source = "userOfTheDamageMeter";
                    Target = line.Substring(indexOfTarget, lengthOfTarget);
                    Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = "Nano";

                    break;

                //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                case "09":

                    indexOfSource = 0;
                    lengthOfSource = line.IndexOf(" hit ") - indexOfSource;
                    // + 5 to skip the " hit " indices
                    indexOfTarget = lengthOfSource + 5;
                    lengthOfTarget = line.IndexOf(" for ") - indexOfTarget;
                    // + 5 to skip the " for " indices
                    indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                    lengthOfAmount = line.LastIndexOf(" points ") - indexOfAmount;
                    // + 8 to skip the " points " indices, + 3 to skip the "of " indices
                    indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                    lengthOfAmountType = line.Length - 8 - indexOfAmountType;

                    //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                    if (line[line.Length - 1] == '!')
                    {
                        lengthOfAmountType = line.Length - 22 - indexOfAmountType;
                        Modifier = "Crit";
                    }
                    //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                    else if (line[line.Length - 2] == 't')
                    {
                        lengthOfAmountType = line.Length - 22 - indexOfAmountType;
                        Modifier = "Glance";
                    }

                    Action = "PetDamage";
                    Source = line.Substring(indexOfSource, lengthOfSource);
                    Target = line.Substring(indexOfTarget, lengthOfTarget);
                    Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = line.Substring(indexOfAmountType, lengthOfAmountType);
                    Source = line.Substring(indexOfSource, lengthOfSource);

                    break;


                //You lost 1548140 xp.
                case "0b":
                    /*
                    indexOfTimeStart = 37;
                    
                    */
                    break;

                //Executing Nano Program: Composite Attribute Boost.
                //Wait for current nano program execution to finish.
                //Unable to execute nano program. You can't execute this nano on the target.
                case "18":
                    /*
                    indexOfTimeStart = 40;
                    indexOfMessageStart = indexOfTimeStart + timeLength + 1;

                    eventTime = Convert.ToInt32(line.Substring(indexOfTimeStart, timeLength));
                    */
                    break;


            }
        }
    }
}
