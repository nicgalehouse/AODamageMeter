using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class YourMisses : AttackEvent
    {
        public const string EventName = "Your misses";

        public static readonly Regex
            Typed =   CreateRegex($"You try to attack {TARGET} with {DAMAGETYPE}, but you miss!"),
            Untyped = CreateRegex($"You tried to hit {TARGET}, but missed!");

        public YourMisses(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static YourMisses Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new YourMisses(fight, timestamp, description);
            attackEvent.SetSourceToOwner();
            attackEvent.AttackResult = AttackResult.Missed;

            if (attackEvent.TryMatch(Typed, out Match match))
            {
                attackEvent.SetTarget(match, 1);
                attackEvent.SetDamageType(match, 2);
            }
            else if (attackEvent.TryMatch(Untyped, out match))
            {
                attackEvent.SetTarget(match, 1);
            }
            else attackEvent.IsUnmatched = true;

            return attackEvent;
        }
    }
}
