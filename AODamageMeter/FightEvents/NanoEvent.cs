using System;

namespace AODamageMeter.FightEvents
{
    public abstract class NanoEvent : FightEvent
    {
        protected NanoEvent(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public string NanoProgram { get; protected set; }
        public bool IsStartOfCast { get; protected set; }
        public CastResult? CastResult { get; protected set; }
        public NanoEvent EndEvent { get; protected set; }
    }
}
