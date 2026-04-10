using AODamageMeter.FightEvents;
using AODamageMeter.FightEvents.Attack;
using AODamageMeter.FightEvents.Heal;
using AODamageMeter.FightEvents.Level;
using AODamageMeter.Helpers;
using System;
using System.Text.RegularExpressions;

namespace AODamageMeter
{
    public abstract class FightEvent
    {
        protected const string SOURCE = "(.+?)", TARGET = "(.+?)", AMOUNT = @"(\d+)";

        protected FightEvent(Fight fight, DateTime timestamp, string description, long logUnixSeconds)
        {
            Fight = fight;
            Timestamp = timestamp;
            Description = description;
            LogUnixSeconds = logUnixSeconds;
        }

        protected FightEvent(Fight fight, DateTime timestamp, LogEntry logEntry)
            : this(fight, timestamp, logEntry.Description, logEntry.UnixSeconds)
        { }

        public static FightEvent Create(Fight fight, LogEntry logEntry)
        {
            DateTime timestamp = !fight.DamageMeter.IsSummaryMode ? DateTime.Now : logEntry.Timestamp;

            switch (logEntry.EventName)
            {
                case MeCastNano.EventName: return new MeCastNano(fight, timestamp, logEntry);
                case MeGotHealth.EventName: return new MeGotHealth(fight, timestamp, logEntry);
                case MeGotNano.EventName: return new MeGotNano(fight, timestamp, logEntry);
                case MeGotSK.EventName: return new MeGotSK(fight, timestamp, logEntry);
                case MeGotXP.EventName: return new MeGotXP(fight, timestamp, logEntry);
                case MeHitByEnvironment.EventName: return new MeHitByEnvironment(fight, timestamp, logEntry);
                case MeHitByMonster.EventName: return new MeHitByMonster(fight, timestamp, logEntry);
                case MeHitByNano.EventName: return new MeHitByNano(fight, timestamp, logEntry);
                case MeHitByPlayer.EventName: return new MeHitByPlayer(fight, timestamp, logEntry);
                case OtherHitByNano.EventName: return new OtherHitByNano(fight, timestamp, logEntry);
                case OtherHitByOther.EventName: return new OtherHitByOther(fight, timestamp, logEntry);
                case OtherMisses.EventName: return new OtherMisses(fight, timestamp, logEntry);
                case Research.EventName: return new Research(fight, timestamp, logEntry);
                case SystemEvent.EventName: return new SystemEvent(fight, timestamp, logEntry);
                case VicinityEvent.EventName: return new VicinityEvent(fight, timestamp, logEntry);
                case YouGaveHealth.EventName: return new YouGaveHealth(fight, timestamp, logEntry);
                case YouGaveNano.EventName: return new YouGaveNano(fight, timestamp, logEntry);
                case YouHitOther.EventName: return new YouHitOther(fight, timestamp, logEntry);
                case YouHitOtherWithNano.EventName: return new YouHitOtherWithNano(fight, timestamp, logEntry);
                case YourMisses.EventName: return new YourMisses(fight, timestamp, logEntry);
                case YourPetHitByMonster.EventName: return new YourPetHitByMonster(fight, timestamp, logEntry);
                case YourPetHitByNano.EventName: return new YourPetHitByNano(fight, timestamp, logEntry);
                case YourPetHitByOther.EventName: return new YourPetHitByOther(fight, timestamp, logEntry);
                default: return new UnrecognizedEvent(fight, timestamp, logEntry);
            }
        }

        public DamageMeter DamageMeter => Fight.DamageMeter;
        public Fight Fight { get; }
        public abstract string Name { get; }
        public virtual bool CanStartFight => true;
        public DateTime Timestamp { get; }
        public string Description { get; }
        public long LogUnixSeconds { get; }
        public bool IsUnmatched { get; protected set; }
        public FightCharacter Source { get; protected internal set; }
        public FightCharacter Target { get; protected internal set; }
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
            string uncoloredName = NameHelper.UncolorName(name);
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
            string uncoloredName = NameHelper.UncolorName(name);
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

        /* It's convenient to have a source and a target for every attack event, so to achieve that we sometimes use a character
           named 〈Unknown〉. Then for example we can have a single TotalDamage value at the fight level, instead of two slightly
           different values, TotalDamageDone and TotalDamageTaken. */

        protected void SetSourceToUnknown()
            => Source = Fight.GetOrCreateFightCharacter(FightCharacter.UnknownCharacterName, Timestamp);

        protected void SetTargetToUnknown()
            => Target = Fight.GetOrCreateFightCharacter(FightCharacter.UnknownCharacterName, Timestamp);

        protected void SetSourceAndTargetToUnknown()
            => Source = Target = Fight.GetOrCreateFightCharacter(FightCharacter.UnknownCharacterName, Timestamp);

        protected void SetAmount(Match match, int index)
            => Amount = int.Parse(match.Groups[index].Value);

        public override string ToString()
            => $"{Name}: {Description}";
    }
}
