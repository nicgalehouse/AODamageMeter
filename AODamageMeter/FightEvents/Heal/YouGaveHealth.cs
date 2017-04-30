using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Heal
{
    // To tie back to the conversation in MeGotHealth, this tells us the potential amount you healed someone.
    // It sucks that we can't get the realized amount too, or just get only the realized amount.
    public class YouGaveHealth : HealEvent
    {
        public const string EventName = "You gave health";

        public static readonly Regex
            Normal = CreateRegex($"You healed {TARGET} for {AMOUNT} points of health.");

        public YouGaveHealth(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static async Task<YouGaveHealth> Create(Fight fight, DateTime timestamp, string description)
        {
            var healEvent = new YouGaveHealth(fight, timestamp, description);
            healEvent.SetSourceToOwner();
            healEvent.HealType = HealType.PotentialHealth;

            if (healEvent.TryMatch(Normal, out Match match))
            {
                await healEvent.SetTarget(match, 1).ConfigureAwait(false);
                healEvent.SetAmount(match, 2);
            }
            else healEvent.IsUnmatched = true;

            return healEvent;
        }
    }
}
