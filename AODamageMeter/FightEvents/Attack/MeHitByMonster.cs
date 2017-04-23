using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class MeHitByMonster : AttackEvent
    {
        public const string EventName = "Me hit by monster";

        public static readonly Regex
            Normal =  CreateRegex($"{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Crit =    CreateRegex($"{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage. Critical hit!", rightToLeft: true),
            Glance =  CreateRegex($"{SOURCE} hit you for {AMOUNT} points of {DAMAGETYPE} damage. Glancing hit.", rightToLeft: true),
            Reflect = CreateRegex($"Someone's reflect shield hit you for {AMOUNT} points of damage."),
            Shield =  CreateRegex($"Someone's damage shield hit you for {AMOUNT} points of damage."),
            Absorb =  CreateRegex($"You absorbed {AMOUNT} points of {DAMAGETYPE} damage.");

        protected MeHitByMonster(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static async Task<MeHitByMonster> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new MeHitByMonster(fight, timestamp, description);
            attackEvent.SetTargetToOwner();

            bool crit = false, glance = false, reflect = false, shield = false;
            if (attackEvent.TryMatch(Normal, out Match match, out bool normal)
                || attackEvent.TryMatch(Crit, out match, out crit)
                || attackEvent.TryMatch(Glance, out match, out glance))
            {
                // Don't deduce anything about the character type here. Could be from a pet of a player with the same name,
                // and in that case we want to count the pet's damage in with the player's. Could be a pet following the
                // other naming convention for pets, and in that case we want to keep the type as pet. If neither of those,
                // it's already an NPC, so there's nothing to do.
                await attackEvent.SetSource(match, 1).ConfigureAwait(false);
                attackEvent.AttackResult = AttackResult.Hit;
                attackEvent.SetAmount(match, 2);
                attackEvent.SetDamageType(match, 3);
                attackEvent.AttackModifier = crit ? AODamageMeter.AttackModifier.Crit
                    : glance ? AODamageMeter.AttackModifier.Glance
                    : (AttackModifier?)null;
            }
            else if (attackEvent.TryMatch(Reflect, out match, out reflect)
                || attackEvent.TryMatch(Shield, out match, out shield))
            {
                attackEvent.AttackResult = AttackResult.IndirectHit;
                attackEvent.SetAmount(match, 1);
                attackEvent.DamageType = reflect ? AODamageMeter.DamageType.Reflect : AODamageMeter.DamageType.Shield;
            }
            else if (attackEvent.TryMatch(Absorb, out match))
            {
                attackEvent.AttackResult = AttackResult.Absorbed;
                attackEvent.SetAmount(match, 1);
                attackEvent.SetDamageType(match, 2);
            }
            else attackEvent.IsUnmatched = true;

            return attackEvent;
        }
    }
}
