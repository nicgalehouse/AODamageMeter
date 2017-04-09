using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents.Attack
{
    public class MeHitByMonster : AttackEvent
    {
        public const string EventKey = "06";
        public const string EventName = "Me hit by monster";

        public static readonly Regex
            Normal =  CreateRegex(@"(.+?) hit you for (\d+) points of (.+?) damage.", rightToLeft: true),
            Crit =    CreateRegex(@"(.+?) hit you for (\d+) points of (.+?) damage. Critical hit!", rightToLeft: true),
            Glance =  CreateRegex(@"(.+?) hit you for (\d+) points of (.+?) damage. Glancing hit.", rightToLeft: true),
            Reflect = CreateRegex(@"Someone's reflect shield hit you for (\d+) points of damage."),
            Shield =  CreateRegex(@"Someone's damage shield hit you for (\d+) points of damage."),
            Absorb =  CreateRegex(@"You absorbed (\d+) points of (.+?) damage.");

        protected MeHitByMonster(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<MeHitByMonster> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var attackEvent = new MeHitByMonster(damageMeter, fight, timestamp, description);
            attackEvent.SetTargetToOwner();

            bool crit = false, glance = false, reflect = false, shield = false;
            if (attackEvent.TryMatch(Normal, out Match match, out bool normal)
                || attackEvent.TryMatch(Crit, out match, out crit)
                || attackEvent.TryMatch(Glance, out match, out glance))
            {
                await attackEvent.SetSource(match, 1);
                attackEvent.AttackResult = AttackResult.DirectHit;
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
            else throw new NotSupportedException($"{EventName}: {description}");

            return attackEvent;
        }
    }
}
