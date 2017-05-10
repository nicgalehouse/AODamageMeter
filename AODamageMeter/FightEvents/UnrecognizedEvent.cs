using System;

namespace AODamageMeter.FightEvents
{
    public class UnrecognizedEvent : FightEvent
    {
        public const string EventName = "Unrecognized";
        public override string Name => EventName;

        public UnrecognizedEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
            => IsUnmatched = true;
    }
}
