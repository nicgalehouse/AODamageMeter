using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Heal
{
    public class MeGotHealth : HealEvent
    {
        public const string EventName = "Me got health";

        public static readonly Regex
            Unsourced = CreateRegex($"You were healed for {AMOUNT} points."),
            Sourced =   CreateRegex($"You got healed by {SOURCE} for {AMOUNT} points of health.");

        protected MeGotHealth(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static async Task<MeGotHealth> Create(Fight fight, DateTime timestamp, string description)
        {
            var healEvent = new MeGotHealth(fight, timestamp, description);
            healEvent.SetTargetToOwner();
            healEvent.HealType = HealType.Health;

            if (healEvent.TryMatch(Unsourced, out Match match))
            {
                // Usually this is from you healing yourself, which is a useful metric, so assume it's the case.
                healEvent.SetSourceToOwner();
                healEvent.SetAmount(match, 1);
            }
            else if (healEvent.TryMatch(Sourced, out match))
            {
                await healEvent.SetSource(match, 1);
                healEvent.SetAmount(match, 2);
            }
            else healEvent.Unmatched = true;

            return healEvent;
        }
    }
}
