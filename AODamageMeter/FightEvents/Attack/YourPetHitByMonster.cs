using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    // Just rely on standard naming conventions to deduce pet characters. Otherwise we'd have to worry more about
    // name collisions, I think. For example, your pet has the default name, we pick it up here, then the chance
    // is pretty high there are going to be other default-named pets that aren't yours, but will be attributed to
    // you. Only using the naming conventions is a way to guide users down a less error-prone path.
    public class YourPetHitByMonster : AttackEvent
    {
        public const string EventName = "Your pet hit by monster";
        public override string Name => EventName;

        public static readonly Regex
            Environment = CreateRegex($"Your pet {TARGET} was damaged by a toxic substance for {AMOUNT} points of damage.");

        public YourPetHitByMonster(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            if (TryMatch(Environment, out Match match))
            {
                SetTarget(match, 1);
                AttackResult = AttackResult.IndirectHit;
                SetAmount(match, 2);
                DamageType = AODamageMeter.DamageType.Environment;
            }
            else IsUnmatched = true;
        }
    }
}
