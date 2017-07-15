using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    public class YouHitOtherWithNano : AttackEvent
    {
        public const string EventName = "You hit other with nano";
        public override string Name => EventName;

        public static readonly Regex
            Basic = CreateRegex($"You hit {TARGET} with nanobots for {AMOUNT} points of {DAMAGETYPE} damage.");

        public YouHitOtherWithNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetSourceToOwner();
            AttackResult = AttackResult.NanoHit;

            if (TryMatch(Basic, out Match match))
            {
                SetTarget(match, 1);
                SetAmount(match, 2);
                SetDamageType(match, 3);
            }
            else IsUnmatched = true;
        }
    }
}
