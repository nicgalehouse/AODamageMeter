using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Heal
{
    public class MeGotNano : HealEvent
    {
        public const string EventName = "Me got nano";
        public override string Name => EventName;

        public static readonly Regex
            Basic = CreateRegex($"You got nano from {SOURCE} for {AMOUNT} points.");

        public MeGotNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetTargetToOwner();
            HealType = HealType.Nano;

            if (TryMatch(Basic, out Match match))
            {
                // TODO: is the source always a player character?
                SetSource(match, 1);
                SetAmount(match, 2);
            }
            else IsUnmatched = true;
        }

        public MeGotNano(SystemEvent selfNanoHealEvent)
            : base(selfNanoHealEvent.Fight, selfNanoHealEvent.Timestamp, selfNanoHealEvent.Description)
        {
            Source = Target = selfNanoHealEvent.Source;
            HealType = HealType.Nano;
            Amount = selfNanoHealEvent.Amount;
        }
    }
}
