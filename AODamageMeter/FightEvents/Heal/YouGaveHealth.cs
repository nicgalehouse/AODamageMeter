using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Heal
{
    // To tie back to the conversation in MeGotHealth, this tells us the potential amount you healed someone.
    // It sucks that we can't get the realized amount too, or just get only the realized amount.
    public class YouGaveHealth : HealEvent
    {
        public const string EventName = "You gave health";
        public override string Name => EventName;

        public static readonly Regex
            Normal = CreateRegex($"You healed {TARGET} for {AMOUNT} points of health.");

        public YouGaveHealth(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetSourceToOwner();
            HealType = HealType.PotentialHealth;

            if (TryMatch(Normal, out Match match))
            {
                SetTarget(match, 1);
                SetAmount(match, 2);
            }
            else IsUnmatched = true;
        }
    }
}
