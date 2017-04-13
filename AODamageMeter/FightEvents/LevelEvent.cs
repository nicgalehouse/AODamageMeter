using System;

namespace AODamageMeter.FightEvents
{
    public abstract class LevelEvent : FightEvent
    {
        protected LevelEvent(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public LevelType LevelType { get; protected set; }
    }
}
