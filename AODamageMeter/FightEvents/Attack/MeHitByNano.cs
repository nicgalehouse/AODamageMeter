using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class MeHitByNano : AttackEvent
    {
        public const string EventName = "Me hit by nano";

        public static readonly Regex
            Sourced =   CreateRegex($"You were attacked with nanobots from {SOURCE} for {AMOUNT} points of {DAMAGETYPE} damage."),
            Unsourced = CreateRegex($"You were attacked with nanobots for {AMOUNT} points of {DAMAGETYPE} damage.");

        public MeHitByNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static async Task<MeHitByNano> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new MeHitByNano(fight, timestamp, description);
            attackEvent.SetTargetToOwner();
            attackEvent.AttackResult = AttackResult.DirectHit;

            if (attackEvent.TryMatch(Sourced, out Match match))
            {
                await attackEvent.SetSource(match, 1);
                attackEvent.SetAmount(match, 2);
                attackEvent.SetDamageType(match, 3);
            }
            else if (attackEvent.TryMatch(Unsourced, out match))
            {
                attackEvent.SetAmount(match, 1);
                attackEvent.SetDamageType(match, 2);
            }
            else attackEvent.Unmatched = true;

            return attackEvent;
        }
    }
}
