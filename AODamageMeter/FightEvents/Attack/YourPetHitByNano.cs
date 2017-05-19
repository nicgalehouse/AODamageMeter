using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    // Just rely on standard naming conventions to deduce pet characters. Otherwise we'd have to worry more about
    // name collisions, I think. For example, your pet has the default name, we pick it up here, then the chance
    // is pretty high there are going to be other default-named pets that aren't yours, but will be attributed to
    // you. Only using the naming conventions is a way to guide users down a less error-prone path.
    public class YourPetHitByNano : AttackEvent
    {
        public const string EventName = "Your pet hit by nano";
        public override string Name => EventName;

        public static readonly Regex
            Sourced =   CreateRegex($"{TARGET} was attacked with nanobots from {SOURCE} for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Unsourced = CreateRegex($"{TARGET} was attacked with nanobots for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true);

        public YourPetHitByNano(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            AttackResult = AttackResult.NanoHit;

            if (TryMatch(Sourced, out Match match))
            {
                SetSourceAndTarget(match, 2, 1);
                SetAmount(match, 3);
                SetDamageType(match, 4);
            }
            else if (TryMatch(Unsourced, out match))
            {
                SetTarget(match, 1);
                SetAmount(match, 2);
                SetDamageType(match, 3);
            }
            else IsUnmatched = true;
        }
    }
}
