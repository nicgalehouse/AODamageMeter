using System;

namespace AODamageMeter.FightEvents
{
    public abstract class NanoEvent : FightEvent
    {
        protected NanoEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public string NanoProgram { get; protected set; }
        public bool IsStartOfCast => EndEvent != null;
        public bool IsEndOfCast => CastResult.HasValue && !IsStartOfCast; // StartEvent may be null.
        public CastResult? CastResult { get; protected set; }
        public bool IsCastUnavailable { get; protected set; }

        // Both null or either one not null, but never both not null.
        // If this.StartEvent is not null, this.StartEvent.EndEvent == this.
        // If this.EndEvent is not null, this.EndEvent.StartEvent == this.
        public NanoEvent StartEvent { get; protected set; }
        public NanoEvent EndEvent { get; protected set; }
    }
}
