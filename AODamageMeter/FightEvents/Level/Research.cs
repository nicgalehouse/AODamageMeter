using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Level
{
    public class Research : LevelEvent
    {
        public const string EventName = "Research";

        public static readonly Regex
            Allocated = CreateRegex($"{AMOUNT} of your XP were allocated to your personal research."),
            Completed = CreateRegex($"You have completed your research on \"(.+)\"");

        public Research(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;
        public string Line { get; protected set; }

        public static Research Create(Fight fight, DateTime timestamp, string description)
        {
            var levelEvent = new Research(fight, timestamp, description);
            levelEvent.LevelType = LevelType.Research;

            if (levelEvent.TryMatch(Allocated, out Match match))
            {
                levelEvent.SetAmount(match, 1);
            }
            else if (levelEvent.TryMatch(Completed, out match))
            {
                levelEvent.Line = match.Groups[1].Value;
            }
            else levelEvent.Unmatched = true;

            return levelEvent;
        }
    }
}
