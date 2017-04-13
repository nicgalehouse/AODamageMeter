using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Level
{
    public class MeGotSK : LevelEvent
    {
        public const string EventKey = "0c";
        public const string EventName = "Me got SK";

        public static readonly Regex
            Gained = CreateRegex($"You gained {AMOUNT} points of Shadowknowledge."),
            Lost =   CreateRegex($"You lost {AMOUNT} points of Shadowknowledge.");

        public MeGotSK(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static MeGotSK Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var levelEvent = new MeGotSK(damageMeter, fight, timestamp, description);
            levelEvent.LevelType = LevelType.Shadow;

            if (levelEvent.TryMatch(Gained, out Match match))
            {
                levelEvent.SetAmount(match, 1);
            }
            else if (levelEvent.TryMatch(Lost, out match))
            {
                levelEvent.SetAmount(match, 1);
                levelEvent.Amount = -levelEvent.Amount;
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return levelEvent;
        }
    }
}
