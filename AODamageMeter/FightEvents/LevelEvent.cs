using System;

namespace AODamageMeter.FightEvents
{
    public abstract class LevelEvent : FightEvent
    {
        protected LevelEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public LevelType LevelType { get; protected set; }
    }
}
