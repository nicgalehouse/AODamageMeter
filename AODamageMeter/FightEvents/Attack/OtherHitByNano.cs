using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    public class OtherHitByNano : AttackEvent
    {
        public const string EventName = "Other hit by nano";
        public override string Name => EventName;

        public static readonly Regex
            Sourced =   CreateRegex($"{TARGET} was attacked with nanobots from {SOURCE} for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Unsourced = CreateRegex($"{TARGET} was attacked with nanobots for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true);

        public OtherHitByNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            AttackResult = AttackResult.NanoHit;

            if (TryMatch(Sourced, out Match match))
            {
                SetSourceAndTarget(match, 2, 1);
                SetAmount(match, 3);
                SetDamageType(match, 4);
            }
            else if (TryMatch(Unsourced, out match))
            {
                SetSourceToUnknown();
                SetTarget(match, 1);
                SetAmount(match, 2);
                SetDamageType(match, 3);
            }
            else IsUnmatched = true;
        }
    }
}
