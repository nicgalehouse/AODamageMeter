using System;

namespace AODamageMeter.FightEvents
{
    // TODO: Anything useful in here?
    public class SystemEvent : FightEvent
    {
        public const string EventName = "System";
        public override string Name => EventName;

        public SystemEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
            => IsUnmatched = true;
    }
}
