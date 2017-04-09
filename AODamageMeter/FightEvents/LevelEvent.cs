using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents
{
    public abstract class LevelEvent : FightEvent
    {
        protected LevelEvent(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public int? Amount { get; protected set; }
        public LevelType LevelType { get; protected set; }

        protected void SetAmount(Match match, int index)
            => Amount = int.Parse(match.Groups[index].Value);
    }
}
