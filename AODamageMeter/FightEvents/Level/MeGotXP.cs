using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Level
{
    public class MeGotXP : LevelEvent
    {
        public const string EventName = "Me got XP";

        public static readonly Regex
            Received = CreateRegex($"You received {AMOUNT} xp."),
            Alien =    CreateRegex($"You gained {AMOUNT} new Alien Experience Points.", rightToLeft: true),
            PvpDuel =  CreateRegex($"You gained {AMOUNT} PVP Duel Score.", rightToLeft: true),
            PvpSolo =  CreateRegex($"You gained {AMOUNT} PVP Solo Score.", rightToLeft: true),
            PvpTeam =  CreateRegex($"You gained {AMOUNT} PVP Team Score.", rightToLeft: true),
            Lost =     CreateRegex($"You lost {AMOUNT} xp.");

        public MeGotXP(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static MeGotXP Create(Fight fight, DateTime timestamp, string description)
        {
            var levelEvent = new MeGotXP(fight, timestamp, description);
            levelEvent.SetSourceToOwner();

            if (levelEvent.TryMatch(Received, out Match match))
            {
                levelEvent.LevelType = LevelType.Normal;
                levelEvent.SetAmount(match, 1);
            }
            else if (levelEvent.TryMatch(Alien, out match))
            {
                levelEvent.LevelType = LevelType.Alien;
                levelEvent.SetAmount(match, 1);
            }
            else if (levelEvent.TryMatch(PvpDuel, out match))
            {
                levelEvent.LevelType = LevelType.PvpDuel;
                levelEvent.SetAmount(match, 1);
            }
            else if (levelEvent.TryMatch(PvpSolo, out match))
            {
                levelEvent.LevelType = LevelType.PvpSolo;
                levelEvent.SetAmount(match, 1);
            }
            else if (levelEvent.TryMatch(PvpTeam, out match))
            {
                levelEvent.LevelType = LevelType.PvpTeam;
                levelEvent.SetAmount(match, 1);
            }
            else if (levelEvent.TryMatch(Lost, out match))
            {
                levelEvent.LevelType = LevelType.Normal;
                levelEvent.SetAmount(match, 1);
                levelEvent.Amount = -levelEvent.Amount;
            }
            else levelEvent.IsUnmatched = true;

            return levelEvent;
        }
    }
}
