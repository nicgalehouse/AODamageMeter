using AODamageMeter.FightEvents;
using AODamageMeter.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter
{
    public abstract class FightEvent
    {
        private readonly DamageMeter _damageMeter;
        private readonly Fight _fight;

        public FightEvent(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            _damageMeter = damageMeter;
            _fight = fight;
            Timestamp = timestamp;
            Description = description;
            Parse();
        }

        public static FightEvent Create(DamageMeter damageMeter, Fight fight, string line)
        {
            int lastIndexOfArrayPart = line.IndexOf(']');
            string[] arrayPart = line.Substring(1, lastIndexOfArrayPart - 1)
                .Split(',')
                .Select(p => p.Trim(' ', '"'))
                .ToArray();
            string type = arrayPart[1];
            DateTime timestamp = DateTimeHelper.DateTimeLocalFromUnixSeconds(long.Parse(arrayPart[3]));
            string description = line.Substring(lastIndexOfArrayPart + 1);

            if (type == "Other hit by other") return new OtherHitByOther(damageMeter, fight, timestamp, description);
        }

        public DateTime Timestamp { get; }
        public string Description { get; }

        protected abstract void Parse();

        public string ActionType { get; protected set; } //Damage, Nano, Heal, Absorb
        public string Source;
        public string Target;
        public int Amount;
        public string AmountType;
        public string Modifier; //Crit, Glance

        private void SetAttributes()
        {
            int indexOfSource, lengthOfSource,
                indexOfTarget, lengthOfTarget,
                indexOfAmount, lengthOfAmount,
                indexOfAmountType, lengthOfAmountType;

            switch (Key)
            {
                //You hit TARGET with nanobots for AMOUNT points of AMOUNTTYPE damage.
                case "05":
                    {
                        indexOfTarget = 8;
                        lengthOfTarget = Line.IndexOf(" with ") - indexOfTarget;

                        indexOfAmount = indexOfTarget + lengthOfTarget + 19;
                        lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                        indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                        lengthOfAmountType = Line.Length - 8 - indexOfAmountType;

                        ActionType = "Damage";
                        Source = owningCharacterName;
                        Target = Line.Substring(indexOfTarget, lengthOfTarget);
                        Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));
                        AmountType = Line.Substring(indexOfAmountType, lengthOfAmountType);

                        //You hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                        if (Line[Line.Length - 1] == '!')
                        {
                            Modifier = "Crit";
                        }
                        //You hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                        else if (Line[Line.Length - 2] == 't')
                        {
                            Modifier = "Glance";
                        }
                    }

                    break;

                //TARGET was attacked with nanobots from SOURCE for AMOUNT points of AMOUNTTYPE damage.
                //TARGET was attacked with nanobots for AMOUNT points of AMOUNTTYPE damage.
                case "04":

                    indexOfTarget = 0;
                    lengthOfTarget = Line.IndexOf(" was ");

                    //TARGET was attacked with nanobots from SOURCE for AMOUNT points of AMOUNTTYPE damage.
                    if (Line.IndexOf(" from ") != -1)
                    {
                        indexOfSource = indexOfTarget + lengthOfTarget + 33;
                        lengthOfSource = Line.IndexOf(" for ") - indexOfSource;
                        indexOfAmount = indexOfSource + lengthOfSource + 5;
                        lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                        Source = Line.Substring(indexOfSource, lengthOfSource);

                    }
                    //TARGET was attacked with nanobots for AMOUNT points of AMOUNTTYPE damage.
                    else
                    {
                        indexOfAmount = indexOfTarget + lengthOfTarget + 32;
                        lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                        //might not be true
                        Source = "Unknown-Source";

                    }

                    indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                    lengthOfAmountType = Line.Length - 8 - indexOfAmountType;

                    ActionType = "Nano";
                    Target = Line.Substring(indexOfTarget, lengthOfTarget);
                    Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = Line.Substring(indexOfAmountType, lengthOfAmountType);

                    break;

                //You were healed for AMOUNT points.
                //You got healed by SOURCE for AMOUNT points of health.
                case "15":

                    //You were healed for AMOUNT points.
                    if (Line[4] == 'w')
                    {
                        indexOfAmount = 20;
                        lengthOfAmount = Line.LastIndexOf(" ") - indexOfAmount;

                        Source = owningCharacterName;
                        Target = owningCharacterName;

                    }
                    //You got healed by SOURCE for AMOUNT points of health.
                    else
                    {
                        indexOfSource = 18;
                        lengthOfSource = Line.LastIndexOf(" for ") - indexOfSource;

                        indexOfAmount = indexOfSource + lengthOfSource + 5;
                        lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                        Source = Line.Substring(indexOfSource, lengthOfSource);
                        Target = owningCharacterName;

                    }

                    ActionType = "Heal";
                    Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = "Heal";

                    break;

                //You hit TARGET for AMOUNT points of AMOUNTTYPE damage.
                //Your damage shield hit TARGET for AMOUNT points of damage.
                case "08":

                    if (Line.StartsWith("Your"))
                    {
                        indexOfTarget = 23;
                        lengthOfTarget = Line.IndexOf(" for ") - indexOfTarget;

                        indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                        lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                        ActionType = "Damage";
                        Source = owningCharacterName;
                        Target = Line.Substring(indexOfTarget, lengthOfTarget);
                        Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));
                        AmountType = "Shield";
                    }
                    else
                    {
                        indexOfTarget = 8;
                        lengthOfTarget = Line.IndexOf(" for ") - indexOfTarget;

                        indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                        lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                        indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                        lengthOfAmountType = Line.Length - 8 - indexOfAmountType;

                        ActionType = "Damage";
                        Source = owningCharacterName;
                        Target = Line.Substring(indexOfTarget, lengthOfTarget);
                        Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));
                        AmountType = Line.Substring(indexOfAmountType, lengthOfAmountType);

                        //You hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                        if (Line[Line.Length - 1] == '!')
                        {
                            Modifier = "Crit";
                        }
                        //You hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                        else if (Line[Line.Length - 2] == 't')
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
                    if (!Line.StartsWith("You absorbed ") && !Line.StartsWith("Someone's"))
                    {

                        indexOfSource = 0;
                        lengthOfSource = Line.IndexOf(" hit ") - indexOfSource;

                        indexOfAmount = indexOfSource + lengthOfSource + 13;
                        lengthOfAmount = Line.LastIndexOf(" points ") - indexOfAmount;

                        indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                        lengthOfAmountType = Line.Length - 8 - indexOfAmountType;

                        ActionType = "Damage";
                        Source = Line.Substring(indexOfSource, lengthOfSource);

                        //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                        if (Line[Line.Length - 1] == '!')
                        {
                            lengthOfAmountType = Line.Length - 22 - indexOfAmountType;
                            Modifier = "Crit";
                        }
                        //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                        else if (Line[Line.Length - 2] == 't')
                        {
                            lengthOfAmountType = Line.Length - 22 - indexOfAmountType;
                            Modifier = "Glance";
                        }
                        AmountType = Line.Substring(indexOfAmountType, lengthOfAmountType);
                    }
                    //You absorbed AMOUNT points of AMOUNTTYPE damage.
                    else if (!Line.StartsWith("Someone's"))
                    {

                        indexOfAmount = 13;
                        lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                        indexOfAmountType = indexOfAmount + 8;
                        lengthOfAmountType = Line.Length - 8 - indexOfAmountType;

                        ActionType = "Absorb";
                        Source = owningCharacterName;
                        AmountType = Line.Substring(indexOfAmountType, lengthOfAmountType);
                    }
                    //Someone's reflect shield hit you for AMOUNT points of damage.
                    //Someone's damage shield hit you for AMOUNT points of damage.
                    else
                    {
                        //Someone's damage shield hit you for AMOUNT points of damage.
                        if (Line[10] == 'd')
                        {
                            indexOfAmount = 36;
                            lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;
                            AmountType = "Shield";
                        }
                        //Someone's reflect shield hit you for AMOUNT points of damage.
                        else
                        {
                            indexOfAmount = 37;
                            lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;
                            AmountType = "Reflect";
                        }
                    }

                    Target = owningCharacterName;
                    Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));

                    break;

                //SOURCE tried to hit you, but missed!
                //SOURCE tries to attack you with Brawl, but misses!
                //SOURCE tries to attack you with FastAttack, but misses!
                //SOURCE tries to attack you with FlingShot, but misses!
                case "13":

                    indexOfSource = 0;

                    if (Line.LastIndexOf("you,") == -1)
                    {

                        lengthOfSource = Line.IndexOf(" tries ") - indexOfSource;
                        //should be changed in future
                        AmountType = "SpecialAttack";

                    }
                    else
                    {
                        lengthOfSource = Line.IndexOf(" tried ") - indexOfSource;
                        AmountType = "Auto Attack";
                    }

                    ActionType = "Damage";
                    Source = Line.Substring(indexOfSource, lengthOfSource);
                    Target = owningCharacterName;

                    break;

                //You got nano from SOURCE for AMOUNT points.
                case "16":

                    indexOfSource = 18;
                    lengthOfSource = Line.IndexOf(" for ") - indexOfSource;

                    indexOfAmount = indexOfSource + lengthOfSource + 5;
                    lengthOfAmount = Line.Length - 8 - indexOfAmount;

                    ActionType = "Heal";
                    Source = Line.Substring(indexOfSource, lengthOfSource);
                    Target = owningCharacterName;
                    Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = "Nano";

                    break;

                //You tried to hit TARGET, but missed!
                case "12":


                    indexOfTarget = 17;
                    lengthOfTarget = Line.Length - 13 - indexOfTarget;

                    ActionType = "Miss";
                    Source = owningCharacterName;
                    Target = Line.Substring(indexOfTarget, lengthOfTarget);

                    break;

                //You increased nano on TARGET for AMOUNT points.
                case "17":

                    indexOfTarget = 22;
                    lengthOfTarget = Line.LastIndexOf(" for ") - indexOfTarget;

                    indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                    lengthOfAmount = Line.Length - 8 - indexOfAmount;

                    ActionType = "Heal";
                    Source = owningCharacterName;
                    Target = Line.Substring(indexOfTarget, lengthOfTarget);
                    Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = "Nano";

                    break;

                //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                case "09":

                    indexOfSource = 0;
                    lengthOfSource = Line.IndexOf(" hit ") - indexOfSource;
                    // + 5 to skip the " hit " indices
                    indexOfTarget = lengthOfSource + 5;
                    lengthOfTarget = Line.IndexOf(" for ") - indexOfTarget;
                    // + 5 to skip the " for " indices
                    indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                    lengthOfAmount = Line.LastIndexOf(" points ") - indexOfAmount;
                    // + 8 to skip the " points " indices, + 3 to skip the "of " indices
                    indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                    lengthOfAmountType = Line.Length - 8 - indexOfAmountType;

                    //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                    if (Line[Line.Length - 1] == '!')
                    {
                        lengthOfAmountType = Line.Length - 22 - indexOfAmountType;
                        Modifier = "Crit";
                    }
                    //SOURCE hit you for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                    else if (Line[Line.Length - 2] == 't')
                    {
                        lengthOfAmountType = Line.Length - 22 - indexOfAmountType;
                        Modifier = "Glance";
                    }
                    
                    ActionType = "Damage"; //PetDamage
                    Source = Line.Substring(indexOfSource, lengthOfSource);
                    Target = Line.Substring(indexOfTarget, lengthOfTarget);
                    Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = Line.Substring(indexOfAmountType, lengthOfAmountType);
                    Source = Line.Substring(indexOfSource, lengthOfSource);

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

                    ActionType = "Utility";
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
