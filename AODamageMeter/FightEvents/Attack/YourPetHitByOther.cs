using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class YourPetHitByOther : AttackEvent
    {
        public const string EventName = "Your pet hit by other";

        public static readonly Regex
            Normal =  CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Crit =    CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Critical hit!", rightToLeft: true),
            Glance =  CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Glancing hit.", rightToLeft: true),
            Reflect = CreateRegex($"{SOURCE}'s reflect shield hit {TARGET} for {AMOUNT} points of damage.", rightToLeft: true),
            Shield =  CreateRegex($"{SOURCE}'s damage shield hit {TARGET} for {AMOUNT} points of damage.", rightToLeft: true);

        public YourPetHitByOther(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        // This is actually "your pet hit by other" and "other hit by your pet", we can't determine if the pet is the source or
        // the target in general, so we use naming conventions higher up. See the comment about pets in FightEvent.cs.
        public static async Task<YourPetHitByOther> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new YourPetHitByOther(fight, timestamp, description);

            bool crit = false, glance = false, reflect = false, shield = false;
            if (attackEvent.TryMatch(Normal, out Match match, out bool normal)
                || attackEvent.TryMatch(Crit, out match, out crit)
                || attackEvent.TryMatch(Glance, out match, out glance))
            {
                await attackEvent.SetSourceAndTarget(match, 1, 2);
                attackEvent.AttackResult = AttackResult.DirectHit;
                attackEvent.SetAmount(match, 3);
                attackEvent.SetDamageType(match, 4);
                attackEvent.AttackModifier = crit ? AODamageMeter.AttackModifier.Crit
                    : glance ? AODamageMeter.AttackModifier.Glance
                    : (AttackModifier?)null;
            }
            else if (attackEvent.TryMatch(Reflect, out match, out reflect)
                || attackEvent.TryMatch(Shield, out match, out shield))
            {
                await attackEvent.SetSourceAndTarget(match, 1, 2);
                attackEvent.AttackResult = AttackResult.IndirectHit;
                attackEvent.SetAmount(match, 3);
                attackEvent.DamageType = reflect ? AODamageMeter.DamageType.Reflect : AODamageMeter.DamageType.Shield;
            }
            else attackEvent.Unmatched = true;

            return attackEvent;
        }
    }
}
