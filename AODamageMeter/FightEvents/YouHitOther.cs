using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DamageTypes = AODamageMeter.DamageType;
using Modifiers = AODamageMeter.Modifier;

namespace AODamageMeter.FightEvents
{
    public class YouHitOther : FightEvent
    {
        public const string EventKey = "08";
        public const string EventName = "You hit other";

        public static readonly Regex
            Normal =  new Regex(@"^You hit (.+?) for (\d+) points of (.+?) damage.$", RegexOptions.Compiled),
            Crit =    new Regex(@"^You hit (.+?) for (\d+) points of (.+?) damage. Critical hit!$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Glance =  new Regex(@"^You hit (.+?) for (\d+) points of (.+?) damage. Glancing hit.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Reflect = new Regex(@"^Your reflect shield hit (.+?) for (\d+) points of damage.$", RegexOptions.Compiled),
            Shield =  new Regex(@"^Your damage shield hit (.+?) for (\d+) points of damage.$", RegexOptions.Compiled);

        protected YouHitOther(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<YouHitOther> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var fightEvent = new YouHitOther(damageMeter, fight, timestamp, description);

            bool crit = false, glance = false, reflect = false, shield = false;
            if (fightEvent.TryMatch(Normal, out Match match, out bool normal)
                || fightEvent.TryMatch(Crit, out match, out crit)
                || fightEvent.TryMatch(Glance, out match, out glance))
            {
                fightEvent.SetSourceToOwner();
                await fightEvent.SetTarget(match, 1);
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
                fightEvent.SetSourceToOwner();
                await fightEvent.SetTarget(match, 1);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 2);
                fightEvent.DamageType = reflect ? DamageTypes.Reflect : DamageTypes.Shield;
            }
            else throw new NotSupportedException($"{EventName}: {description}");

            return fightEvent;
        }
    }
}
