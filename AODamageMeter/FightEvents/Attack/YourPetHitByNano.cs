using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class YourPetHitByNano : AttackEvent
    {
        public const string EventName = "Your pet hit by nano";

        public static readonly Regex
            Sourced =   CreateRegex($"{TARGET} was attacked with nanobots from {SOURCE} for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Unsourced = CreateRegex($"{TARGET} was attacked with nanobots for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true);

        protected YourPetHitByNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        // Just rely on standard naming conventions to deduce pet characters. Otherwise we'd have to worry more about
        // name collisions, I think. For example, your pet has the default name, we pick it up here, then the chance
        // is pretty high there are going to be other default-named pets that aren't yours, but will be attributed to
        // you. Only using the naming conventions is a way to guide users down a less error-prone path.
        public static async Task<YourPetHitByNano> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new YourPetHitByNano(fight, timestamp, description);
            attackEvent.AttackResult = AttackResult.Hit;

            if (attackEvent.TryMatch(Sourced, out Match match))
            {
                await attackEvent.SetSourceAndTarget(match, 2, 1);
                attackEvent.SetAmount(match, 3);
                attackEvent.SetDamageType(match, 4);
            }
            else if (attackEvent.TryMatch(Unsourced, out match))
            {
                await attackEvent.SetTarget(match, 1);
                attackEvent.SetAmount(match, 2);
                attackEvent.SetDamageType(match, 3);
            }
            else attackEvent.Unmatched = true;

            return attackEvent;
        }
    }
}
