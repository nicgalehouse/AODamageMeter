using System;

namespace AODamageMeter.FightEvents
{
    // SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage.
    // SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
    // SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
    // SOURCE's reflect shield hit TARGET for AMOUNT points of damage.
    // SOURCE's damage shield hit TARGET for AMOUNT points of damage.
    // Something hit TARGET for AMOUNT points of damage by damage shield.
    // Something hit TARGET for AMOUNT points of damage by reflect shield.
    // Someone absorbed AMOUNT points of AMOUNTTYPE damage.
    public class OtherHitByOther : FightEvent
    {
        public OtherHitByOther(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        protected override void Parse()
        {
            // We don't have enough info about this event to do anything.
            // Someone absorbed AMOUNT points of AMOUNTTYPE damage.
            if (Description.Contains("Someone absorbed "))
            {
                ActionType = "Absorb";
                return;
            }

            // SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage.
            if (Line.IndexOf(" shield hit ") == -1 && Line.IndexOf("shield.") == -1)
            {
                indexOfSource = 0;
                lengthOfSource = Line.IndexOf(" hit ") - indexOfSource;
                // + 5 to skip the " hit " indices
                indexOfTarget = indexOfSource + lengthOfSource + 5;
                lengthOfTarget = Line.IndexOf(" for ") - indexOfTarget;

                indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                indexOfAmountType = indexOfAmount + lengthOfAmount + 11;
                lengthOfAmountType = Line.IndexOf(" damage.") - indexOfAmountType;

                AmountType = Line.Substring(indexOfAmountType, lengthOfAmountType);
                Source = Line.Substring(indexOfSource, lengthOfSource);

                //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Critical hit!
                if (Line[Line.Length - 1] == '!')
                {
                    Modifier = "Crit";
                }
                //SOURCE hit TARGET for AMOUNT points of AMOUNTTYPE damage. Glancing hit.
                else if (Line[Line.Length - 2] == 't')
                {
                    Modifier = "Glance";
                }
            }
            //SOURCE's reflect shield hit TARGET for AMOUNT points of damage.
            //SOURCE's damage shield hit TARGET for AMOUNT points of damage.
            else if (Line.IndexOf(" shield hit ") != -1)
            {
                indexOfSource = 0;
                //SOURCE's reflect shield hit TARGET for AMOUNT points of damage.
                if (Line.IndexOf(" reflect ") != -1)
                {
                    lengthOfSource = Line.IndexOf(" reflect shield ") - 2 - indexOfSource;
                    indexOfTarget = indexOfSource + lengthOfSource + 22;
                    AmountType = "Reflect";
                }
                //SOURCE's damage shield hit TARGET for AMOUNT points of damage.
                else
                {
                    lengthOfSource = Line.IndexOf(" damage shield ") - 2 - indexOfSource;
                    indexOfTarget = indexOfSource + lengthOfSource + 21;
                    AmountType = "Shield";
                }

                lengthOfTarget = Line.IndexOf(" for ") - indexOfTarget;

                indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                Source = Line.Substring(indexOfSource, lengthOfSource);
            }
            //Something hit TARGET for AMOUNT points of damage by damage shield.
            //Something hit TARGET for AMOUNT points of damage by reflect shield.
            else
            {
                indexOfTarget = 14;
                lengthOfTarget = Line.IndexOf(" for ") - indexOfTarget;

                indexOfAmount = indexOfTarget + lengthOfTarget + 5;
                lengthOfAmount = Line.IndexOf(" points ") - indexOfAmount;

                //Something hit TARGET for AMOUNT points of damage by damage shield.
                if (Line[Line.Length - 9] == 'e')
                {
                    AmountType = "Shield";
                }
                //Something hit TARGET for AMOUNT points of damage by reflect shield.
                else
                {
                    AmountType = "Reflect";
                }

            }

            Target = Line.Substring(indexOfTarget, lengthOfTarget);
            Amount = Convert.ToInt32(Line.Substring(indexOfAmount, lengthOfAmount));
            ActionType = "Damage";

            break;
        }
    }
}
