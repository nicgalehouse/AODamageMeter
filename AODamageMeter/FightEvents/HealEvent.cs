using System;

namespace AODamageMeter.FightEvents
{
    public abstract class HealEvent : FightEvent
    {
        protected HealEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public HealType HealType { get; protected set; }
        public bool IsStartOfHeal => HealType == HealType.PotentialHealth;
        public bool IsEndOfHeal => StartEvent != null; // HealType.RealizedHealth implicitly.
        public bool IsSelfContainedHeal => !IsStartOfHeal && !IsEndOfHeal; // <==> HealType.RealizedHealth and StartEvent == null.

        // Both null or either one not null, but never both not null.
        // If this.StartEvent is not null, this.StartEvent.EndEvent == this.
        // If this.EndEvent is not null, this.EndEvent.StartEvent == this.
        public HealEvent StartEvent { get; protected set; }
        public HealEvent EndEvent { get; protected set; }
    }
}
