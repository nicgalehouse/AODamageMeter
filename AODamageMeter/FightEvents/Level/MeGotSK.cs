using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Level
{
    public class MeGotSK : LevelEvent
    {
        public const string EventName = "Me got SK";
        public override string Name => EventName;

        public static readonly Regex
            Gained = CreateRegex($"You gained {AMOUNT} points of Shadowknowledge."),
            Lost =   CreateRegex($"You lost {AMOUNT} points of Shadowknowledge.");

        public MeGotSK(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetSourceToOwner();
            LevelType = LevelType.Shadow;

            if (TryMatch(Gained, out Match match))
            {
                SetAmount(match, 1);
            }
            else if (TryMatch(Lost, out match))
            {
                SetAmount(match, 1);
                Amount *= -1;
            }
            else IsUnmatched = true;
        }
    }
}
