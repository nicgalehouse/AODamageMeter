using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    public class YourMisses : AttackEvent
    {
        public const string EventName = "Your misses";
        public override string Name => EventName;

        public static readonly Regex
            Typed =   CreateRegex($"You try to attack {TARGET} with {DAMAGETYPE}, but you miss!"),
            Untyped = CreateRegex($"You tried to hit {TARGET}, but missed!");

        public YourMisses(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetSourceToOwner();
            AttackResult = AttackResult.Missed;

            if (TryMatch(Typed, out Match match))
            {
                SetTarget(match, 1);
                SetDamageType(match, 2);
            }
            else if (TryMatch(Untyped, out match))
            {
                SetTarget(match, 1);
            }
            else IsUnmatched = true;
        }
    }
}
