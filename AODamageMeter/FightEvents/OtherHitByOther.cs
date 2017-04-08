using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents
{
    public class OtherHitByOther : FightEvent
    {
        protected static readonly Regex
            _normal =       new Regex(@"^(.+?) hit (.+?) for (\d+) points of (.+?) damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _crit =         new Regex(@"^(.+?) hit (.+?) for (\d+) points of (.+?) damage. Critical hit!$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _glance =       new Regex(@"^(.+?) hit (.+?) for (\d+) points of (.+?) damage. Glancing hit.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _reflect =      new Regex(@"^(.+?)'s reflect shield hit (.+?) for (\d+) points of damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _shield =       new Regex(@"^(.+?)'s damage shield hit (.+?) for (\d+) points of damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _weirdReflect = new Regex(@"^Something hit (.+?) for (\d+) points of damage by reflect shield.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _weirdShield =  new Regex(@"^Something hit (.+?) for (\d+) points of damage by damage shield.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _absorb =       new Regex(@"^Someone absorbed (\d+) points of (.+?) damage.$", RegexOptions.Compiled);

        protected OtherHitByOther(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public static async Task<OtherHitByOther> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var @event = new OtherHitByOther(damageMeter, fight, timestamp, description);

            bool crit = false, glance = false, reflect = false, shield = false, weirdReflect = false, weirdShield = false, absorb = false;
            if (@event.TryMatch(_normal, out Match match, out bool normal)
                || @event.TryMatch(_crit, out match, out crit)
                || @event.TryMatch(_glance, out match, out glance))
            {
                var characters = await Character.GetOrCreateCharacters(match.Groups[1].Value, match.Groups[2].Value);
                @event.Source = characters[0];
                @event.Target = characters[1];
                @event.ActionType = ActionType.Damage;
                @event.Amount = int.Parse(match.Groups[3].Value);
                @event.AmountType = match.Groups[4].Value.GetAmountType();
                @event.Modifier = crit ? AODamageMeter.Modifier.Crit
                    : glance ? AODamageMeter.Modifier.Glance
                    : (Modifier?)null;
            }
            else if (@event.TryMatch(_reflect, out match, out reflect)
                || @event.TryMatch(_shield, out match, out shield))
            {
            }
        }

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
