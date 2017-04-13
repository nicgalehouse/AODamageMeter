using AODamageMeter.FightEvents.Attack;
using AODamageMeter.FightEvents.Heal;
using AODamageMeter.FightEvents.Level;
using AODamageMeter.FightEvents.Nano;
using AODamageMeter.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public abstract class FightEvent
    {
        protected const string SOURCE = "(.+?)", TARGET = "(.+?)", AMOUNT = @"(\d+)", DAMAGETYPE = "(.+?)";
        protected readonly DamageMeter _damageMeter;
        protected readonly Fight _fight;

        protected FightEvent(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            _damageMeter = damageMeter;
            _fight = fight;
            Timestamp = timestamp;
            Description = description;
        }

        public static async Task<FightEvent> Create(DamageMeter damageMeter, Fight fight, string line)
        {
            int lastIndexOfArrayPart = line.IndexOf(']');
            string[] arrayPart = line.Substring(1, lastIndexOfArrayPart - 1).Split(',')
                .Select(p => p.Trim('"'))
                .ToArray();
            string eventName = arrayPart[1];
            DateTime timestamp = DateTimeHelper.DateTimeLocalFromUnixSeconds(long.Parse(arrayPart[3]));
            string description = line.Substring(lastIndexOfArrayPart + 1);

            if (eventName == MeCastNano.EventName) return MeCastNano.Create(damageMeter, fight, timestamp, description);
            if (eventName == MeGotHealth.EventName) return await MeGotHealth.Create(damageMeter, fight, timestamp, description);
            if (eventName == MeGotNano.EventName) return await MeGotNano.Create(damageMeter, fight, timestamp, description);
            if (eventName == MeGotSK.EventName) return MeGotSK.Create(damageMeter, fight, timestamp, description);
            if (eventName == MeGotXP.EventName) return MeGotXP.Create(damageMeter, fight, timestamp, description);
            if (eventName == MeHitByMonster.EventName) return await MeHitByMonster.Create(damageMeter, fight, timestamp, description);
            if (eventName == OtherHitByNano.EventName) return await OtherHitByNano.Create(damageMeter, fight, timestamp, description);
            if (eventName == OtherHitByOther.EventName) return await OtherHitByOther.Create(damageMeter, fight, timestamp, description);
            if (eventName == YouHitOther.EventName) return await YouHitOther.Create(damageMeter, fight, timestamp, description);
            if (eventName == YouHitOtherWithNano.EventName) return await YouHitOtherWithNano.Create(damageMeter, fight, timestamp, description);
            throw new NotSupportedException($"{eventName}: {description}");
        }

        public abstract string Key { get; }
        public abstract string Name { get; }
        public DateTime Timestamp { get; }
        public string Description { get; }
        public FightCharacter Source { get; protected set; }
        public FightCharacter Target { get; protected set; }
        public int? Amount { get; protected set; }

        protected static Regex CreateRegex(string body, bool rightToLeft = false)
            => new Regex($"^{body}$", rightToLeft ? RegexOptions.Compiled | RegexOptions.RightToLeft : RegexOptions.Compiled);

        protected bool TryMatch(Regex regex, out Match match)
            => (match = regex.Match(Description)).Success;

        protected bool TryMatch(Regex regex, out Match match, out bool success)
        {
            match = regex.Match(Description);
            success = match.Success;
            return success;
        }

        protected async Task SetSource(Match match, int index, CharacterType? knownCharacterType = null)
        {
            Source = await _fight.GetOrCreateFightCharacter(match.Groups[index].Value);
            Source.Character.CharacterType = knownCharacterType ?? Source.Character.CharacterType;
        }

        protected async Task SetTarget(Match match, int index)
            => Target = await _fight.GetOrCreateFightCharacter(match.Groups[index].Value);

        protected async Task SetSourceAndTarget(Match match, int sourceIndex, int targetIndex)
        {
            var fightCharacters = await _fight.GetOrCreateFightCharacters(match.Groups[sourceIndex].Value, match.Groups[targetIndex].Value);
            Source = fightCharacters[0];
            Target = fightCharacters[1];
        }

        protected void SetSourceToOwner()
            => Source = _fight.GetOrCreateFightCharacter(_damageMeter.Owner);

        protected void SetTargetToOwner()
            => Target = _fight.GetOrCreateFightCharacter(_damageMeter.Owner);

        protected void SetSourceAndTargetToOwner()
            => Source = Target = _fight.GetOrCreateFightCharacter(_damageMeter.Owner);

        protected void SetAmount(Match match, int index)
            => Amount = int.Parse(match.Groups[index].Value);











        private void SetAttributes()
        {
            int indexOfSource, lengthOfSource,
                indexOfTarget, lengthOfTarget,
                indexOfAmount, lengthOfAmount,
                indexOfAmountType, lengthOfAmountType;

            switch (Key)
            {
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
                        DamageType = "SpecialAttack";

                    }
                    else
                    {
                        lengthOfSource = Line.IndexOf(" tried ") - indexOfSource;
                        DamageType = "Auto Attack";
                    }

                    ActionType = "Damage";
                    Source = Line.Substring(indexOfSource, lengthOfSource);
                    Target = owningCharacterName;

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
                    DamageType = "Nano";

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
                    DamageType = Line.Substring(indexOfAmountType, lengthOfAmountType);
                    Source = Line.Substring(indexOfSource, lengthOfSource);

                    break;
            }
        }
    }
}
