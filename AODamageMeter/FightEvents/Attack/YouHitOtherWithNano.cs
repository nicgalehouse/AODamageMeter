using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class YouHitOtherWithNano : AttackEvent
    {
        public const string EventName = "You hit other with nano";

        public static readonly Regex
            Normal = CreateRegex($"You hit {TARGET} with nanobots for {AMOUNT} points of {DAMAGETYPE} damage.");

        protected YouHitOtherWithNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static async Task<YouHitOtherWithNano> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new YouHitOtherWithNano(fight, timestamp, description);
            attackEvent.SetSourceToOwner();
            attackEvent.AttackResult = AttackResult.Hit;

            if (attackEvent.TryMatch(Normal, out Match match))
            {
                await attackEvent.SetTarget(match, 1).ConfigureAwait(false);
                attackEvent.SetAmount(match, 2);
                attackEvent.SetDamageType(match, 3);
            }
            else attackEvent.IsUnmatched = true;

            return attackEvent;
        }
    }
}
