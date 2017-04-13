using System;

namespace AODamageMeter
{
    public abstract class HealEvent : FightEvent
    {
        protected HealEvent(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public HealType HealType { get; protected set; }
    }
}
