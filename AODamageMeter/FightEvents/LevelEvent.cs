using System;

namespace AODamageMeter.FightEvents
{
    public abstract class LevelEvent : FightEvent
    {
        protected LevelEvent(Fight fight, DateTime timestamp, LogEntry logEntry)
            : base(fight, timestamp, logEntry)
        { }

        public LevelType LevelType { get; protected set; }
    }
}
