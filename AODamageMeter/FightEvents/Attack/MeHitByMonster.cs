using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    public class MeHitByMonster : AttackEvent
    {
        public const string EventName = "Me hit by monster";
        public override string Name => EventName;

        public static readonly Regex
            Normal =  CreateRegex($"{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Crit =    CreateRegex($"{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage. Critical hit!", rightToLeft: true),
            Glance =  CreateRegex($"{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage. Glancing hit.", rightToLeft: true),
            Reflect = CreateRegex($"Someone's reflect shield hit you for {AMOUNT} points of damage."),
            Shield =  CreateRegex($"Someone's damage shield hit you for {AMOUNT} points of damage."),
            Absorb =  CreateRegex($"You absorbed {AMOUNT} points of {DAMAGETYPE} damage.");

        public MeHitByMonster(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            SetTargetToOwner();

            bool crit = false, glance = false, reflect = false, shield = false;
            if (TryMatch(Normal, out Match match, out bool normal)
                || TryMatch(Crit, out match, out crit)
                || TryMatch(Glance, out match, out glance))
            {
                // Don't deduce anything about the character type here. Could be from a pet of a player with the same name,
                // and in that case we want to count the pet's damage in with the player's. Could be a pet following the
                // other naming convention for pets, and in that case we want to keep the type as pet. If neither of those,
                // it's already an NPC, so there's nothing to do.
                SetSource(match, 1);
                AttackResult = AttackResult.Hit;
                SetAmount(match, 2);
                SetDamageType(match, 3);
                AttackModifier = crit ? AODamageMeter.AttackModifier.Crit
                    : glance ? AODamageMeter.AttackModifier.Glance
                    : (AttackModifier?)null;
            }
            else if (TryMatch(Reflect, out match, out reflect)
                || TryMatch(Shield, out match, out shield))
            {
                AttackResult = AttackResult.IndirectHit;
                SetAmount(match, 1);
                DamageType = reflect ? AODamageMeter.DamageType.Reflect : AODamageMeter.DamageType.Shield;
            }
            else if (TryMatch(Absorb, out match))
            {
                AttackResult = AttackResult.Absorbed;
                SetAmount(match, 1);
                SetDamageType(match, 2);
            }
            else IsUnmatched = true;
        }
    }
}
