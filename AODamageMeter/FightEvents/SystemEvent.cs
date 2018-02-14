using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents
{
    public class SystemEvent : FightEvent
    {
        public const string EventName = "System";
        public override string Name => EventName;

        public static readonly Regex
            SelfNanoHeal =       CreateRegex($"You increased your nanopool with {AMOUNT} points."),
            HealthDrain =        CreateRegex($"You drained {AMOUNT} points of health from the target."),
            NanoDrain =          CreateRegex($"You drained {AMOUNT} points of nano from the target."),
            NanoInterrupt =      CreateRegex($"Your nano execution got interrupted by (.+).."),
            YouBlockedRegular =  CreateRegex($"Your attack shield blocked the attack! \\(.\\) left."),
            YourRegularBlocked = CreateRegex($"Your attack was blocked by an attack shield!");

        public bool IsSelfNanoHeal { get; protected set; }
        public bool IsHealthDrain { get; protected set; }
        public bool IsNanoDrain { get; protected set; }
        public bool IsNanoInterrupt { get; protected set; }
        public bool IsYouBlockedRegular { get; protected set; }
        public bool IsYourRegularBlocked { get; protected set; }

        public SystemEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            if (TryMatch(SelfNanoHeal, out Match match))
            {
                IsSelfNanoHeal = true;
                SetAmount(match, 1);
            }
            else if (TryMatch(HealthDrain, out match))
            {
                IsHealthDrain = true;
                SetAmount(match, 1);
            }
            else if (TryMatch(NanoDrain, out match))
            {
                IsNanoDrain = true;
                SetAmount(match, 1);
            }
            else if (TryMatch(NanoInterrupt, out match))
            {
                IsNanoInterrupt = true;
            }
            else if (TryMatch(YouBlockedRegular, out match))
            {
                IsYouBlockedRegular = true;
            }
            else if (TryMatch(YourRegularBlocked, out match))
            {
                IsYourRegularBlocked = true;
            }
            else IsUnmatched = true;
        }
    }
}
