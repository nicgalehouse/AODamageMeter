using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Level
{
    public class Research : LevelEvent
    {
        public const string EventName = "Research";
        public override string Name => EventName;

        public static readonly Regex
            Allocated = CreateRegex($"{AMOUNT} of your XP were allocated to your personal research.<br>", rightToLeft: true),
            Completed = CreateRegex($"You have completed your research on \"(.+)\"");

        public Research(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetSourceToOwner();
            LevelType = LevelType.Research;

            if (TryMatch(Allocated, out Match match))
            {
                SetAmount(match, 1);
            }
            else if (TryMatch(Completed, out match))
            {
                Line = match.Groups[1].Value;
            }
            else IsUnmatched = true;
        }

        public string Line { get; protected set; }
    }
}
