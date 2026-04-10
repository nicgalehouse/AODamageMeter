using System;

namespace AODamageMeter.FightEvents
{
    public class VicinityEvent : FightEvent
    {
        public const string EventName = "Vicinity";
        public override string Name => EventName;
        public override bool CanStartFight => false;

        public VicinityEvent(Fight fight, DateTime timestamp, LogEntry logEntry)
            : base(fight, timestamp, logEntry)
            => IsUnmatched = true;
    }
}
