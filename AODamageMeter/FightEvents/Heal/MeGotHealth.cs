using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Heal
{
    // When someone heals you (via a nano, HoT, first aid, treatment kit, etc), a sourced event gets created with
    // the amount that you could've been healed for, if you were hurt enough, and an unsourced event gets created
    // with the amount you were actually healed for (if you weren't hurt at all, no events get created). When you
    // heal yourself (via a nano, heal delta, etc), there's a single unsourced event. I'm going to assume that the
    // events are always in order--that if there's a sourced event, the corresponding unsourced event immediately
    // follows it in the chain of MeGotHealth events. Seems to be the case, but not sure.
    public class MeGotHealth : HealEvent
    {
        public const string EventName = "Me got health";
        protected static MeGotHealth _latestStartEvent;

        public static readonly Regex
            Sourced =   CreateRegex($"You got healed by {SOURCE} for {AMOUNT} points of health."),
            Unsourced = CreateRegex($"You were healed for {AMOUNT} points.");

        protected MeGotHealth(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static async Task<MeGotHealth> Create(Fight fight, DateTime timestamp, string description)
        {
            var healEvent = new MeGotHealth(fight, timestamp, description);
            healEvent.SetTargetToOwner();

            if (healEvent.TryMatch(Sourced, out Match match))
            {
                await healEvent.SetSource(match, 1).ConfigureAwait(false);
                healEvent.HealType = HealType.PotentialHealth;
                healEvent.SetAmount(match, 2);
                _latestStartEvent = healEvent;
            }
            else if (healEvent.TryMatch(Unsourced, out match))
            {
                healEvent.HealType = HealType.RealizedHealth;
                healEvent.SetAmount(match, 1);

                if (_latestStartEvent != null)
                {
                    healEvent.Source = _latestStartEvent.Source;

                    healEvent.StartEvent = _latestStartEvent;
                    _latestStartEvent.EndEvent = healEvent;
                    _latestStartEvent = null;
                }
                else
                {
                    healEvent.SetSourceToOwner();
                }
            }
            else healEvent.IsUnmatched = true;

            return healEvent;
        }
    }
}
