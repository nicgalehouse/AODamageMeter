using System;

namespace AODamageMeter.FightEvents
{
    public class UnrecognizedEvent : FightEvent
    {
        public const string EventName = "Unrecognized";
        public override string Name => EventName;
        public override bool CanStartFight => false;

        public UnrecognizedEvent(Fight fight, DateTime timestamp, LogEntry logEntry)
            : base(fight, timestamp, logEntry)
            => IsUnmatched = true;
    }
}
