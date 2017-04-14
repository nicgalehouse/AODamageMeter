using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Level
{
    public class MeGotXP : LevelEvent
    {
        public const string EventName = "Me got XP";

        public static readonly Regex
            Received = CreateRegex($"You received {AMOUNT} xp."),
            Lost =     CreateRegex($"You lost {AMOUNT} xp."),
            Alien =    CreateRegex($"You gained {AMOUNT} new Alien Experience Points.", rightToLeft: true),
            PvpSolo =  CreateRegex($"You gained {AMOUNT} PVP Solo Score.", rightToLeft: true),
            PvpTeam =  CreateRegex($"You gained {AMOUNT} PVP Team Score.", rightToLeft: true);

        public MeGotXP(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static MeGotXP Create(Fight fight, DateTime timestamp, string description)
        {
            var levelEvent = new MeGotXP(fight, timestamp, description);

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
            else levelEvent.Unmatched = true;

            return levelEvent;
        }
    }
}
