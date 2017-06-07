using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    public class MeHitByEnvironment : AttackEvent
    {
        public const string EventName = "Me hit by environment";
        public override string Name => EventName;

        public static readonly Regex
            Normal = CreateRegex($"You were damaged by a toxic substance for {AMOUNT} points of damage.");

        public MeHitByEnvironment(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetSourceToUnknown();
            SetTargetToOwner();
            AttackResult = AttackResult.IndirectHit;
            DamageType = AODamageMeter.DamageType.Environment;

            if (TryMatch(Normal, out Match match))
            {
                SetAmount(match, 1);
            }
            else IsUnmatched = true;
        }
    }
}
