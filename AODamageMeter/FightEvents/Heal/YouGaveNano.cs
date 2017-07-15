using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Heal
{
    public class YouGaveNano : HealEvent
    {
        public const string EventName = "You gave nano";
        public override string Name => EventName;

        public static readonly Regex
            Basic = CreateRegex($"You increased nano on {TARGET} for {AMOUNT} points.");

        public YouGaveNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetSourceToOwner();
            HealType = HealType.Nano;

            if (TryMatch(Basic, out Match match))
            {
                SetTarget(match, 1);
                SetAmount(match, 2);
            }
            else IsUnmatched = true;
        }
    }
}
