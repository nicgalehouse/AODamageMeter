using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    // This is actually "your pet hit by other" and "other hit by your pet", we can't determine if the pet is the source or
    // the target in general, so we use naming conventions higher up. See the comment in Character.
    public class YourPetHitByOther : AttackEvent
    {
        public const string EventName = "Your pet hit by other";
        public override string Name => EventName;

        public static readonly Regex
            Normal =  CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Crit =    CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Critical hit!", rightToLeft: true),
            Glance =  CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Glancing hit.", rightToLeft: true),
            Reflect = CreateRegex($"{SOURCE}'s reflect shield hit {TARGET} for {AMOUNT} points of damage.", rightToLeft: true),
            Shield =  CreateRegex($"{SOURCE}'s damage shield hit {TARGET} for {AMOUNT} points of damage.", rightToLeft: true);

        public YourPetHitByOther(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            bool crit = false, glance = false, reflect = false, shield = false;
            if (TryMatch(Normal, out Match match, out bool normal)
                || TryMatch(Crit, out match, out crit)
                || TryMatch(Glance, out match, out glance))
            {
                SetSourceAndTarget(match, 1, 2);
                AttackResult = AttackResult.WeaponHit;
                SetAmount(match, 3);
                SetDamageType(match, 4);
                AttackModifier = crit ? AODamageMeter.AttackModifier.Crit
                    : glance ? AODamageMeter.AttackModifier.Glance
                    : (AttackModifier?)null;
            }
            else if (TryMatch(Reflect, out match, out reflect)
                || TryMatch(Shield, out match, out shield))
            {
                SetSourceAndTarget(match, 1, 2);
                AttackResult = AttackResult.IndirectHit;
                SetAmount(match, 3);
                DamageType = reflect ? AODamageMeter.DamageType.Reflect : AODamageMeter.DamageType.Shield;
            }
            else IsUnmatched = true;
        }
    }
}
