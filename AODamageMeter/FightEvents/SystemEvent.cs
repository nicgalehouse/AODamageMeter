using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents
{
    public class SystemEvent : FightEvent
    {
        public const string EventName = "System";
        public override string Name => EventName;
        public override bool CanStartFight
            => !IsUnmatched && !IsNanoDeactivated && !IsNanoTerminated && !IsFriendlyNanoExecutedOnYou;

        public static readonly Regex
            SelfNanoHeal =              CreateRegex($"You increased your nanopool with {AMOUNT} points."),
            HealthDrain =               CreateRegex($"You drained {AMOUNT} points of health from the target."),
            NanoDrain =                 CreateRegex($"You drained {AMOUNT} points of nano from the target."),
            NanoInterrupt =             CreateRegex($"Your nano execution got interrupted by (.+).."),
            YouBlockedRegular =         CreateRegex($"Your attack shield blocked the attack! \\(.\\) left."),
            YourRegularBlocked =        CreateRegex($"Your attack was blocked by an attack shield!"),
            NanoDeactivated =           CreateRegex($"Deactivating friendly timed nanoprogram."),
            NanoTerminated =            CreateRegex($"Nanoprogram (.+) terminated..."),
            FriendlyNanoExecutedOnYou = CreateRegex($"{SOURCE} executes (.+) within your NCU..."),
            HostileNanoExecutedOnYou =  CreateRegex($"{SOURCE} forces your NCU to run (.+)..."),
            HostileNanoCounteredByYou = CreateRegex($"You countered {SOURCE}'s attempt to run (.+) within your NCU.");

        public bool IsSelfNanoHeal { get; protected set; }
        public bool IsHealthDrain { get; protected set; }
        public bool IsNanoDrain { get; protected set; }
        public bool IsNanoInterrupt { get; protected set; }
        public bool IsYouBlockedRegular { get; protected set; }
        public bool IsYourRegularBlocked { get; protected set; }
        public bool IsNanoDeactivated { get; protected set; }
        public bool IsNanoTerminated { get; protected set; }
        public bool IsFriendlyNanoExecutedOnYou { get; protected set; }
        public bool IsHostileNanoExecutedOnYou { get; protected set; }
        public bool IsHostileNanoCounteredByYou { get; protected set; }
        public string NanoProgram { get; protected set; }

        public SystemEvent(Fight fight, DateTime timestamp, LogEntry logEntry)
            : base(fight, timestamp, logEntry)
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
            else if (TryMatch(NanoDeactivated, out match))
            {
                IsNanoDeactivated = true;
            }
            else if (TryMatch(NanoTerminated, out match))
            {
                IsNanoTerminated = true;
                NanoProgram = match.Groups[1].Value;
            }
            else if (TryMatch(FriendlyNanoExecutedOnYou, out match))
            {
                IsFriendlyNanoExecutedOnYou = true;
                SetSource(match, 1);
                NanoProgram = match.Groups[2].Value;
            }
            else if (TryMatch(HostileNanoExecutedOnYou, out match))
            {
                IsHostileNanoExecutedOnYou = true;
                SetSource(match, 1);
                NanoProgram = match.Groups[2].Value;
            }
            else if (TryMatch(HostileNanoCounteredByYou, out match))
            {
                IsHostileNanoCounteredByYou = true;
                SetSource(match, 1);
                NanoProgram = match.Groups[2].Value;
            }
            else IsUnmatched = true;
        }
    }
}
