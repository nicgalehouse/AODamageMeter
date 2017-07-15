using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    public class OtherMisses : AttackEvent
    {
        public const string EventName = "Other misses";
        public override string Name => EventName;

        public static readonly Regex
            Basic =   CreateRegex($"{SOURCE} tried to hit you, but missed!", rightToLeft: true),
            Special = CreateRegex($"{SOURCE} tries to attack you with {DAMAGETYPE}, but misses!", rightToLeft: true);

        public OtherMisses(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetTargetToOwner();
            AttackResult = AttackResult.Missed;

            if (TryMatch(Basic, out Match match))
            {
                SetSource(match, 1);
            }
            else if (TryMatch(Special, out match))
            {
                SetSource(match, 1);
                SetDamageType(match, 2);
            }
            else IsUnmatched = true;
        }
    }
}
