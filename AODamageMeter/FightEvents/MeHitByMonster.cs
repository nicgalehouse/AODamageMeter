using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DamageTypes = AODamageMeter.DamageType;
using Modifiers = AODamageMeter.Modifier;

namespace AODamageMeter.FightEvents
{
    public class MeHitByMonster : FightEvent
    {
        public const string EventKey = "06";
        public const string EventName = "Me hit by monster";

        public static readonly Regex
            Normal =  new Regex(@"^(.+?) hit you for (\d+) points of (.+?) damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Crit =    new Regex(@"^(.+?) hit you for (\d+) points of (.+?) damage. Critical hit!$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Glance =  new Regex(@"^(.+?) hit you for (\d+) points of (.+?) damage. Glancing hit.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Reflect = new Regex(@"^Someone's reflect shield hit you for (\d+) points of damage.$", RegexOptions.Compiled),
            Shield =  new Regex(@"^Someone's damage shield hit you for (\d+) points of damage.$", RegexOptions.Compiled),
            Absorb =  new Regex(@"^You absorbed (\d+) points of (.+?) damage.$", RegexOptions.Compiled);

        protected MeHitByMonster(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<MeHitByMonster> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var fightEvent = new MeHitByMonster(damageMeter, fight, timestamp, description);

            bool crit = false, glance = false, reflect = false, shield = false;
            if (fightEvent.TryMatch(Normal, out Match match, out bool normal)
                || fightEvent.TryMatch(Crit, out match, out crit)
                || fightEvent.TryMatch(Glance, out match, out glance))
            {
                await fightEvent.SetSource(match, 1);
                fightEvent.SetTargetToOwner();
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 2);
                fightEvent.SetDamageType(match, 3);
                fightEvent.Modifier = crit ? Modifiers.Crit
                    : glance ? Modifiers.Glance
                    : (Modifiers?)null;
            }
            else if (fightEvent.TryMatch(Reflect, out match, out reflect)
                || fightEvent.TryMatch(Shield, out match, out shield))
            {
                fightEvent.SetTargetToOwner();
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 1);
                fightEvent.DamageType = reflect ? DamageTypes.Reflect : DamageTypes.Shield;
            }
            else if (fightEvent.TryMatch(Absorb, out match))
            {
                fightEvent.SetTargetToOwner();
                fightEvent.ActionType = ActionType.Absorb;
                fightEvent.SetAmount(match, 1);
                fightEvent.SetDamageType(match, 2);
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return fightEvent;
        }
    }
}
