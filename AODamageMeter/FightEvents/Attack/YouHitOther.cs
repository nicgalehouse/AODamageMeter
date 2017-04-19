using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class YouHitOther : AttackEvent
    {
        public const string EventName = "You hit other";

        public static readonly Regex
            Normal =  CreateRegex($"You hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage."),
            Crit =    CreateRegex($"You hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Critical hit!", rightToLeft: true),
            Glance =  CreateRegex($"You hit {TARGET} for {AMOUNT} points of {DAMAGETYPE} damage. Glancing hit.", rightToLeft: true),
            Reflect = CreateRegex($"Your reflect shield hit {TARGET} for {AMOUNT} points of damage."),
            Shield =  CreateRegex($"Your damage shield hit {TARGET} for {AMOUNT} points of damage.");

        protected YouHitOther(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public override string Name => EventName;

        public static async Task<YouHitOther> Create(Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new YouHitOther(fight, timestamp, description);
            attackEvent.SetSourceToOwner();

            bool crit = false, glance = false, reflect = false, shield = false;
            if (attackEvent.TryMatch(Normal, out Match match, out bool normal)
                || attackEvent.TryMatch(Crit, out match, out crit)
                || attackEvent.TryMatch(Glance, out match, out glance))
            {
                await attackEvent.SetTarget(match, 1);
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
                await attackEvent.SetTarget(match, 1);
                attackEvent.AttackResult = AttackResult.IndirectHit;
                attackEvent.SetAmount(match, 2);
                attackEvent.DamageType = reflect ? AODamageMeter.DamageType.Reflect : AODamageMeter.DamageType.Shield;
            }
            else attackEvent.Unmatched = true;

            return attackEvent;
        }
    }
}
