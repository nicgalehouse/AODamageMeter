using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using AODamageMeter.FightEvents.Heal;
using AODamageMeter.FightEvents.Level;
using AODamageMeter.FightEvents.Nano;
using AODamageMeter.Helpers;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AODamageMeter
{
    public abstract class FightEvent
    {
        protected const string SOURCE = "(.+?)", TARGET = "(.+?)", AMOUNT = @"(\d+)";

        protected FightEvent(Fight fight, DateTime timestamp, string description)
        {
            Fight = fight;
            Timestamp = timestamp;
            Description = description;
        }

        public static FightEvent Create(Fight fight, string line)
        {
            // Lines look like this for example, the array part always having four elements:
            // ["#000000004200000a#","Other hit by other","",1492309026]Elitengi hit Leet for 6 points of cold damage.
            int lastIndexOfArrayPart = line.IndexOf(']');
            string[] arrayPart = line.Substring(1, lastIndexOfArrayPart - 1).Split(',')
                .Select(p => p.Trim('"'))
                .ToArray();
            string eventName = arrayPart[1];
            DateTime timestamp = fight.DamageMeter.IsRealTimeMode ? DateTime.Now
                : DateTimeHelper.DateTimeLocalFromUnixSeconds(long.Parse(arrayPart[3]));
            string description = line.Substring(lastIndexOfArrayPart + 1);

            switch (eventName)
            {
                case MeCastNano.EventName: return new MeCastNano(fight, timestamp, description);
                case MeGotHealth.EventName: return new MeGotHealth(fight, timestamp, description);
                case MeGotNano.EventName: return new MeGotNano(fight, timestamp, description);
                case MeGotSK.EventName: return new MeGotSK(fight, timestamp, description);
                case MeGotXP.EventName: return new MeGotXP(fight, timestamp, description);
                case MeHitByEnvironment.EventName: return new MeHitByEnvironment(fight, timestamp, description);
                case MeHitByMonster.EventName: return new MeHitByMonster(fight, timestamp, description);
                case MeHitByNano.EventName: return new MeHitByNano(fight, timestamp, description);
                case MeHitByPlayer.EventName: return new MeHitByPlayer(fight, timestamp, description);
                case OtherHitByNano.EventName: return new OtherHitByNano(fight, timestamp, description);
                case OtherHitByOther.EventName: return new OtherHitByOther(fight, timestamp, description);
                case OtherMisses.EventName: return new OtherMisses(fight, timestamp, description);
                case Research.EventName: return new Research(fight, timestamp, description);
                case SystemEvent.EventName: return new SystemEvent(fight, timestamp, description);
                case YouGaveHealth.EventName: return new YouGaveHealth(fight, timestamp, description);
                case YouGaveNano.EventName: return new YouGaveNano(fight, timestamp, description);
                case YouHitOther.EventName: return new YouHitOther(fight, timestamp, description);
                case YouHitOtherWithNano.EventName: return new YouHitOtherWithNano(fight, timestamp, description);
                case YourMisses.EventName: return new YourMisses(fight, timestamp, description);
                case YourPetHitByMonster.EventName: return new YourPetHitByMonster(fight, timestamp, description);
                case YourPetHitByNano.EventName: return new YourPetHitByNano(fight, timestamp, description);
                case YourPetHitByOther.EventName: return new YourPetHitByOther(fight, timestamp, description);
                default: return new UnrecognizedEvent(fight, timestamp, description);
            }
        }

        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Fight Fight { get; }
        public abstract string Name { get; }
        public DateTime Timestamp { get; }
        public string Description { get; }
        public bool IsUnmatched { get; protected set; }
        public FightCharacter Source { get; protected set; }
        public FightCharacter Target { get; protected set; }
        public int? Amount { get; protected set; }

        protected static Regex CreateRegex(string body, bool rightToLeft = false)
            => new Regex($"^{body}$", rightToLeft ? RegexOptions.Compiled | RegexOptions.RightToLeft : RegexOptions.Compiled);

        protected bool TryMatch(Regex regex, out Match match)
            => (match = regex.Match(Description)).Success;

        protected bool TryMatch(Regex regex, out Match match, out bool success)
            => success = (match = regex.Match(Description)).Success;

        /* The owner's name never appears in the logs for their own events, just the contextual "You". So if we find the owner's
           name (potentially colored), assume it's because they've renamed their pets to match their name. See Character for
           the pet naming conventions, which we follow when creating the fight character for this special case.

           There was a time when we created characters w/ uncolored names, but the context is useful for avoiding false positives.
           Drawback is no aggregation when Elitengi names his robot Elitengi (in red), and same-named rows in the damage meter.
           Oh well to the aggregation but as a todo, we should support colored names in the meter to distinguish same-names. */

        protected void SetSource(Match match, int index)
        {
            string name = match.Groups[index].Value;
            string uncoloredName = Character.UncolorName(name);
            if (uncoloredName == DamageMeter.Owner.Name)
            {
                Source = Fight.GetOrCreateFightCharacter($"{uncoloredName}'s pets", Timestamp);
            }
            else
            {
                Source = Fight.GetOrCreateFightCharacter(name, Timestamp);
            }
        }

        protected void SetTarget(Match match, int index)
        {
            string name = match.Groups[index].Value;
            string uncoloredName = Character.UncolorName(name);
            if (uncoloredName == DamageMeter.Owner.Name)
            {
                Target = Fight.GetOrCreateFightCharacter($"{uncoloredName}'s pets", Timestamp);
            }
            else
            {
                Target = Fight.GetOrCreateFightCharacter(name, Timestamp);
            }
        }

        protected void SetSourceAndTarget(Match match, int sourceIndex, int targetIndex)
        {
            SetSource(match, sourceIndex);
            SetTarget(match, targetIndex);
        }

        protected void SetSourceToOwner()
            => Source = Fight.GetOrCreateFightCharacter(DamageMeter.Owner, Timestamp);

        protected void SetTargetToOwner()
            => Target = Fight.GetOrCreateFightCharacter(DamageMeter.Owner, Timestamp);

        protected void SetSourceAndTargetToOwner()
            => Source = Target = Fight.GetOrCreateFightCharacter(DamageMeter.Owner, Timestamp);

        protected void SetAmount(Match match, int index)
            => Amount = int.Parse(match.Groups[index].Value);

        public override string ToString()
            => $"{Name}: {Description}";
    }
}
