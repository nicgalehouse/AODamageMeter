using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
        public override string Name => EventName;

        protected static readonly ConditionalWeakTable<Fight, MeGotHealth> _latestStartEvents = new ConditionalWeakTable<Fight, MeGotHealth>();

        public static readonly Regex
            Sourced =   CreateRegex($"You got healed by {SOURCE} for {AMOUNT} points of health."),
            Unsourced = CreateRegex($"You were healed for {AMOUNT} points.");

        public MeGotHealth(Fight fight, DateTime timestamp, LogEntry logEntry)
            : base(fight, timestamp, logEntry)
        {
            SetTargetToOwner();

            if (TryMatch(Sourced, out Match match))
            {
                SetSource(match, 1);
                HealType = HealType.PotentialHealth;
                SetAmount(match, 2);
                _latestStartEvents.Remove(fight);
                _latestStartEvents.Add(fight, this);
            }
            else if (TryMatch(Unsourced, out match))
            {
                HealType = HealType.RealizedHealth;
                SetAmount(match, 1);

                if (_latestStartEvents.TryGetValue(fight, out var latestStartEvent))
                {
                    Source = latestStartEvent.Source;

                    StartEvent = latestStartEvent;
                    latestStartEvent.EndEvent = this;
                    _latestStartEvents.Remove(fight);
                }
                else
                {
                    SetSourceToOwner();
                }
            }
            else IsUnmatched = true;
        }
    }
}
