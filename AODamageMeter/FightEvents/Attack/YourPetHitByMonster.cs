using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class YourPetHitByMonster : AttackEvent
    {
        public const string EventName = "Your pet hit by monster";

        public static readonly Regex
            Environment = CreateRegex($"Your pet {TARGET} was damaged by a toxic substance for {AMOUNT} points of damage.");

        public YourPetHitByMonster(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        // Just rely on standard naming conventions to deduce pet characters. Otherwise we'd have to worry more about
        // name collisions, I think. For example, your pet has the default name, we pick it up here, then the chance
        // is pretty high there are going to be other default-named pets that aren't yours, but will be attributed to
        // you. Only using the naming conventions is a way to guide users down a less error-prone path.
        public static async Task<YourPetHitByMonster> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new YourPetHitByMonster(fight, timestamp, description);

            if (attackEvent.TryMatch(Environment, out Match match))
            {
                await attackEvent.SetTarget(match, 1).ConfigureAwait(false);
                attackEvent.AttackResult = AttackResult.IndirectHit;
                attackEvent.SetAmount(match, 2);
                attackEvent.DamageType = AODamageMeter.DamageType.Environment;
            }
            else attackEvent.IsUnmatched = true;

            return attackEvent;
        }
    }
}
