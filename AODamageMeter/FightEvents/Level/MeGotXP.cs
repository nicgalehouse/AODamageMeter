using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Level
{
    public class MeGotXP : LevelEvent
    {
        public const string EventKey = "0b";
        public const string EventName = "Me got XP";

        public static readonly Regex
            Received = CreateRegex($"You received {AMOUNT} xp."),
            Lost =     CreateRegex($"You lost {AMOUNT} xp."),
            Alien =    CreateRegex($"You gained {AMOUNT} new Alien Experience Points."),
            PvpSolo =  CreateRegex($"You gained {AMOUNT} PVP Solo Score."),
            PvpTeam =  CreateRegex($"You gained {AMOUNT} PVP Team Score.");

        public MeGotXP(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static MeGotXP Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var levelEvent = new MeGotXP(damageMeter, fight, timestamp, description);

            if (levelEvent.TryMatch(Received, out Match match))
            {
                levelEvent.SetAmount(match, 1);
                levelEvent.LevelType = LevelType.Normal;
            }
            else if (levelEvent.TryMatch(Lost, out match))
            {
                levelEvent.SetAmount(match, 1);
                levelEvent.Amount = -levelEvent.Amount;
                levelEvent.LevelType = LevelType.Normal;
            }
            else if (levelEvent.TryMatch(Alien, out match))
            {
                levelEvent.SetAmount(match, 1);
                levelEvent.LevelType = LevelType.Alien;
            }
            else if (levelEvent.TryMatch(PvpSolo, out match))
            {
                levelEvent.SetAmount(match, 1);
                levelEvent.LevelType = LevelType.PvpSolo;
            }
            else if (levelEvent.TryMatch(PvpTeam, out match))
            {
                levelEvent.SetAmount(match, 1);
                levelEvent.LevelType = LevelType.PvpTeam;
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return levelEvent;
        }
    }
}
