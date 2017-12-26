using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents
{
    public class SystemEvent : FightEvent
    {
        public const string EventName = "System";
        public override string Name => EventName;

        public static readonly Regex
            SelfNanoHeal =  CreateRegex($"You increased your nanopool with {AMOUNT} points."),
            HealthDrain =   CreateRegex($"You drained {AMOUNT} points of health from the target."),
            NanoDrain =     CreateRegex($"You drained {AMOUNT} points of nano from the target."),
            NanoInterrupt = CreateRegex($"Your nano execution got interrupted by (.+)..");

        public bool IsSelfNanoHeal { get; protected set; }
        public bool IsHealthDrain { get; protected set; }
        public bool IsNanoDrain { get; protected set; }
        public bool IsNanoInterrupt { get; protected set; }

        public SystemEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            if (TryMatch(SelfNanoHeal, out Match match))
            {
                IsSelfNanoHeal = true;
                SetSourceAndTargetToOwner();
                SetAmount(match, 1);
            }
            else if (TryMatch(HealthDrain, out match))
            {
                IsHealthDrain = true;
                SetSourceToOwner();
                SetAmount(match, 1);
            }
            else if (TryMatch(NanoDrain, out match))
            {
                IsNanoDrain = true;
                SetSourceToOwner();
                SetAmount(match, 1);
            }
            else if (TryMatch(NanoInterrupt, out match))
            {
                IsNanoInterrupt = true;
                // Actually owner is target, owner as source is convenient and interrupter wouldn't be used.
                SetSourceToOwner();
            }
            else IsUnmatched = true;
        }
    }
}
