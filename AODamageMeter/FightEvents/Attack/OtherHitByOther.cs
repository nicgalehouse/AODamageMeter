using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents.Attack
{
    public class OtherHitByOther : AttackEvent
    {
        public const string EventName = "Other hit by other";
        public override string Name => EventName;

        public static readonly Regex
            Normal =       CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Crit =         CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Critical hit!", rightToLeft: true),
            Glance =       CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Glancing hit.", rightToLeft: true),
            Reflect =      CreateRegex($"{SOURCE}'s reflect shield hit {TARGET} for {AMOUNT} points of damage.", rightToLeft: true),
            Shield =       CreateRegex($"{SOURCE}'s damage shield hit {TARGET} for {AMOUNT} points of damage.", rightToLeft: true),
            WeirdReflect = CreateRegex($"Something hit {TARGET} for {AMOUNT} points of damage by reflect shield.", rightToLeft: true),
            WeirdShield =  CreateRegex($"Something hit {TARGET} for {AMOUNT} points of damage by damage shield.", rightToLeft: true),
            Absorb =       CreateRegex($"Someone absorbed {AMOUNT} points of {DAMAGETYPE} damage.");

        public OtherHitByOther(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        {
            bool crit = false, glance = false, reflect = false, shield = false, weirdReflect = false, weirdShield = false;
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
            else if (TryMatch(WeirdReflect, out match, out weirdReflect)
                || TryMatch(WeirdShield, out match, out weirdShield))
            {
                SetSourceToUnknown();
                SetTarget(match, 1);
                AttackResult = AttackResult.IndirectHit;
                SetAmount(match, 2);
                DamageType = weirdReflect ? AODamageMeter.DamageType.Reflect : AODamageMeter.DamageType.Shield;
            }
            else if (TryMatch(Absorb, out match))
            {
                SetSourceAndTargetToUnknown();
                AttackResult = AttackResult.Absorbed;
                SetAmount(match, 1);
                SetDamageType(match, 2);
            }
            else IsUnmatched = true;
        }
    }
}
