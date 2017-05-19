using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    public class MeHitByNano : AttackEvent
    {
        public const string EventName = "Me hit by nano";
        public override string Name => EventName;

        public static readonly Regex
            Sourced =   CreateRegex($"You were attacked with nanobots from {SOURCE} for {AMOUNT} points of {DAMAGETYPE} damage."),
            Unsourced = CreateRegex($"You were attacked with nanobots for {AMOUNT} points of {DAMAGETYPE} damage.");

        public MeHitByNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetTargetToOwner();
            AttackResult = AttackResult.NanoHit;

            if (TryMatch(Sourced, out Match match))
            {
                SetSource(match, 1);
                SetAmount(match, 2);
                SetDamageType(match, 3);
            }
            else if (TryMatch(Unsourced, out match))
            {
                SetAmount(match, 1);
                SetDamageType(match, 2);
            }
            else IsUnmatched = true;
        }
    }
}
