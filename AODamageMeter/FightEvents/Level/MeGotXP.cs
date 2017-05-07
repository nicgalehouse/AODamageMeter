using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Level
{
    public class MeGotXP : LevelEvent
    {
        public const string EventName = "Me got XP";
        public override string Name => EventName;

        public static readonly Regex
            Received = CreateRegex($"You received {AMOUNT} xp."),
            Alien =    CreateRegex($"You gained {AMOUNT} new Alien Experience Points.", rightToLeft: true),
            PvpDuel =  CreateRegex($"You gained {AMOUNT} PVP Duel Score.", rightToLeft: true),
            PvpSolo =  CreateRegex($"You gained {AMOUNT} PVP Solo Score.", rightToLeft: true),
            PvpTeam =  CreateRegex($"You gained {AMOUNT} PVP Team Score.", rightToLeft: true),
            Lost =     CreateRegex($"You lost {AMOUNT} xp.");

        public MeGotXP(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetSourceToOwner();

            if (TryMatch(Received, out Match match))
            {
                LevelType = LevelType.Normal;
                SetAmount(match, 1);
            }
            else if (TryMatch(Alien, out match))
            {
                LevelType = LevelType.Alien;
                SetAmount(match, 1);
            }
            else if (TryMatch(PvpDuel, out match))
            {
                LevelType = LevelType.PvpDuel;
                SetAmount(match, 1);
            }
            else if (TryMatch(PvpSolo, out match))
            {
                LevelType = LevelType.PvpSolo;
                SetAmount(match, 1);
            }
            else if (TryMatch(PvpTeam, out match))
            {
                LevelType = LevelType.PvpTeam;
                SetAmount(match, 1);
            }
            else if (TryMatch(Lost, out match))
            {
                LevelType = LevelType.Normal;
                SetAmount(match, 1);
                Amount *= -1;
            }
            else IsUnmatched = true;
        }
    }
}
