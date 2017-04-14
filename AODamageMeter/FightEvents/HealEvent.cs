using System;

namespace AODamageMeter.FightEvents
{
    public abstract class HealEvent : FightEvent
    {
        protected HealEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public HealType HealType { get; protected set; }
    }
}
