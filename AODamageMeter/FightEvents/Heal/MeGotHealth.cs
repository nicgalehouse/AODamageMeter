using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Heal
{
    // When someone heals you (via a nano, HoT, first aid, treatment kit, etc), an unsourced event gets created
    // with the amount you were actually healed for, a sourced event gets created with the amount that you
    // could've been healed for, if you were hurt enough (if you weren't hurt at all, no events). When you
    // heal yourself, there's a single unsourced event. We could try to to connect these events together
    // so we know which unsourced events are from you and which are from other source's, but we're not. So
    // right now, all the unsourced tell you how much you've been healed in total, and all the sourced give
    // you an approximation of how much you've been healed from any specific source.
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
