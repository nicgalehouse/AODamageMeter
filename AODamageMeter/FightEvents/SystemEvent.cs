using System;

namespace AODamageMeter.FightEvents
{
    // TODO: Anything useful in here?
    public class SystemEvent : FightEvent
    {
        public const string EventName = "System";

        public SystemEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static SystemEvent Create(Fight fight, DateTime timestamp, string description)
        {
            var systemEvent = new SystemEvent(fight, timestamp, description);
            systemEvent.IsUnmatched = true;

            return systemEvent;
        }
    }
}
