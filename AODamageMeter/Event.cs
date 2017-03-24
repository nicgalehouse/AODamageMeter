using System;

namespace AODamageMeter
{
    public class Event
    {
        public int Timestamp;
        public string ActionType; //Damage, Nano, Heal, Absorb
        public string Source;
        public string Target;
        public int Amount;
        public string AmountType;
        public string Modifier; //Crit, Glance
        public string Line;


        public Event(string line, string meterOwner)
        {
            Line = line;
            SetAttributes(Key, GetUsefulEventString(), meterOwner);
        }

        // All lines start like this:
        // ["#000000004200000a#"...
        // Last two characters between the number signs seem to be unique.
        private string Key
            => Line?.Substring(17, 2) ?? null;

        public int? GetTime()
        {
            switch (Key)
            {
                case "0b": return int.Parse(Line.Substring(37, 10));
                case "16": return int.Parse(Line.Substring(39, 10));
                case "12": return int.Parse(Line.Substring(39, 10));
                case "13": return int.Parse(Line.Substring(40, 10));
                case "18": return int.Parse(Line.Substring(40, 10));
                case "15": return int.Parse(Line.Substring(41, 10));
                case "08": return int.Parse(Line.Substring(41, 10));
                case "17": return int.Parse(Line.Substring(41, 10));
                case "04": return int.Parse(Line.Substring(45, 10));
                case "06": return int.Parse(Line.Substring(45, 10));
                case "0a": return int.Parse(Line.Substring(46, 10));
                case "09": return int.Parse(Line.Substring(49, 10));
                case "05": return int.Parse(Line.Substring(51, 10));
                default: return null;
            }
        }

        private string GetUsefulEventString()
        {
            switch (Key)
            {
                case "0a": return Line.Substring(57, Line.Length - 57);
                case "05": return Line.Substring(62, Line.Length - 62);
                case "04": return Line.Substring(56, Line.Length - 56);
                case "15": return Line.Substring(52, Line.Length - 52);
                case "08": return Line.Substring(52, Line.Length - 52);
                case "06": return Line.Substring(56, Line.Length - 56);
                case "13": return Line.Substring(51, Line.Length - 51);
                case "16": return Line.Substring(50, Line.Length - 50);
                case "12": return Line.Substring(50, Line.Length - 50);
                case "17": return Line.Substring(52, Line.Length - 52);
                case "09": return Line.Substring(60, Line.Length - 60);
                case "0b": return Line.Substring(48, Line.Length - 48);
                case "18": return Line.Substring(51, Line.Length - 51);
                default: return null;
            }
        }

        private void SetAttributes(string key, string line, string meterOwner)
        {
            if (line == null)
                return;

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
                        ActionType = "Absorb";
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
                    ActionType = "Damage";

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

                        ActionType = "Damage";
                        Source = meterOwner;
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

                    ActionType = "Nano";
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

                        Source = meterOwner;
                        Target = meterOwner;

                    }
                    //You got healed by SOURCE for AMOUNT points of health.
                    else
                    {
                        indexOfSource = 18;
                        lengthOfSource = line.LastIndexOf(" for ") - indexOfSource;

                        indexOfAmount = indexOfSource + lengthOfSource + 5;
                        lengthOfAmount = line.IndexOf(" points ") - indexOfAmount;

                        Source = line.Substring(indexOfSource, lengthOfSource);
                        Target = meterOwner;

                    }

                    ActionType = "Heal";
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

                        ActionType = "Damage";
                        Source = meterOwner;
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

                        ActionType = "Damage";
                        Source = meterOwner;
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

                        ActionType = "Damage";
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

                        ActionType = "Absorb";
                        Source = meterOwner;
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

                    Target = meterOwner;
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

                    ActionType = "Damage";
                    Source = line.Substring(indexOfSource, lengthOfSource);
                    Target = meterOwner;

                    break;

                //You got nano from SOURCE for AMOUNT points.
                case "16":

                    indexOfSource = 18;
                    lengthOfSource = line.IndexOf(" for ") - indexOfSource;

                    indexOfAmount = indexOfSource + lengthOfSource + 5;
                    lengthOfAmount = line.Length - 8 - indexOfAmount;

                    ActionType = "Heal";
                    Source = line.Substring(indexOfSource, lengthOfSource);
                    Target = meterOwner;
                    Amount = Convert.ToInt32(line.Substring(indexOfAmount, lengthOfAmount));
                    AmountType = "Nano";

                    break;

                //You tried to hit TARGET, but missed!
                case "12":


                    indexOfTarget = 17;
                    lengthOfTarget = line.Length - 13 - indexOfTarget;

                    ActionType = "Miss";
                    Source = meterOwner;
                    Target = line.Substring(indexOfTarget, lengthOfTarget);

                    break;

                //You increased nano on TARGET for AMOUNT points.
                case "17":

                    indexOfTarget = 22;
                    lengthOfTarget = line.LastIndexOf(" for ") - indexOfTarget;

                    indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                    lengthOfAmount = line.Length - 8 - indexOfAmount;

                    ActionType = "Heal";
                    Source = meterOwner;
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
                    
                    ActionType = "Damage"; //PetDamage
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
