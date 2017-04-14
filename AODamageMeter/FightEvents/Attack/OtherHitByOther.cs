using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class OtherHitByOther : AttackEvent
    {
        public const string EventKey = "0a";
        public const string EventName = "Other hit by other";

        public static readonly Regex
            Normal =       CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage.", rightToLeft: true),
            Crit =         CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Critical hit!", rightToLeft: true),
            Glance =       CreateRegex($"{SOURCE} hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Glancing hit.", rightToLeft: true),
            Reflect =      CreateRegex($"{SOURCE}'s reflect shield hit {TARGET} for {AMOUNT} points of damage.", rightToLeft: true),
            Shield =       CreateRegex($"{SOURCE}'s damage shield hit {TARGET} for {AMOUNT} points of damage.", rightToLeft: true),
            WeirdReflect = CreateRegex($"Something hit {TARGET} for {AMOUNT} points of damage by reflect shield.", rightToLeft: true),
            WeirdShield =  CreateRegex($"Something hit {TARGET} for {AMOUNT} points of damage by damage shield.", rightToLeft: true),
            Absorb =       CreateRegex($"Someone absorbed {AMOUNT} points of {DAMAGETYPE} damage.");

        protected OtherHitByOther(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<OtherHitByOther> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new OtherHitByOther(fight, timestamp, description);

            bool crit = false, glance = false, reflect = false, shield = false, weirdReflect = false, weirdShield = false;
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
            else if (attackEvent.TryMatch(WeirdReflect, out match, out weirdReflect)
                || attackEvent.TryMatch(WeirdShield, out match, out weirdShield))
            {
                await attackEvent.SetTarget(match, 1);
                attackEvent.AttackResult = AttackResult.IndirectHit;
                attackEvent.SetAmount(match, 2);
                attackEvent.DamageType = weirdReflect ? AODamageMeter.DamageType.Reflect : AODamageMeter.DamageType.Shield;
            }
            else if (attackEvent.TryMatch(Absorb, out match))
            {
                attackEvent.AttackResult = AttackResult.Absorbed;
                attackEvent.SetAmount(match, 1);
                attackEvent.SetDamageType(match, 2);
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return attackEvent;
        }
    }
}
