using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DamageTypes = AODamageMeter.DamageType;
using Modifiers = AODamageMeter.Modifier;

namespace AODamageMeter.FightEvents
{
    public class OtherHitByOther : FightEvent
    {
        public const string EventKey = "0a";
        public const string EventName = "Other hit by other";

        public static readonly Regex
            Normal =       new Regex(@"^(.+?) hit (.+?) for (\d+) points of (.+?) damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Crit =         new Regex(@"^(.+?) hit (.+?) for (\d+) points of (.+?) damage. Critical hit!$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Glance =       new Regex(@"^(.+?) hit (.+?) for (\d+) points of (.+?) damage. Glancing hit.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Reflect =      new Regex(@"^(.+?)'s reflect shield hit (.+?) for (\d+) points of damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Shield =       new Regex(@"^(.+?)'s damage shield hit (.+?) for (\d+) points of damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            WeirdReflect = new Regex(@"^Something hit (.+?) for (\d+) points of damage by reflect shield.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            WeirdShield =  new Regex(@"^Something hit (.+?) for (\d+) points of damage by damage shield.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            Absorb =       new Regex(@"^Someone absorbed (\d+) points of (.+?) damage.$", RegexOptions.Compiled);

        protected OtherHitByOther(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<OtherHitByOther> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var fightEvent = new OtherHitByOther(damageMeter, fight, timestamp, description);

            bool crit = false, glance = false, reflect = false, shield = false, weirdReflect = false, weirdShield = false;
            if (fightEvent.TryMatch(Normal, out Match match, out bool normal)
                || fightEvent.TryMatch(Crit, out match, out crit)
                || fightEvent.TryMatch(Glance, out match, out glance))
            {
                await fightEvent.SetSourceAndTarget(match, 1, 2);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 3);
                fightEvent.SetDamageType(match, 4);
                fightEvent.Modifier = crit ? Modifiers.Crit
                    : glance ? Modifiers.Glance
                    : (Modifiers?)null;
            }
            else if (fightEvent.TryMatch(Reflect, out match, out reflect)
                || fightEvent.TryMatch(Shield, out match, out shield))
            {
                await fightEvent.SetSourceAndTarget(match, 1, 2);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 3);
                fightEvent.DamageType = reflect ? DamageTypes.Reflect : DamageTypes.Shield;
            }
            else if (fightEvent.TryMatch(WeirdReflect, out match, out weirdReflect)
                || fightEvent.TryMatch(WeirdShield, out match, out weirdShield))
            {
                await fightEvent.SetTarget(match, 1);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 2);
                fightEvent.DamageType = weirdReflect ? DamageTypes.Reflect : DamageTypes.Shield;
            }
            else if (fightEvent.TryMatch(Absorb, out match))
            {
                fightEvent.ActionType = ActionType.Absorb;
                fightEvent.SetAmount(match, 1);
                fightEvent.SetDamageType(match, 2);
            }
            else throw new NotSupportedException($"{EventKey}, {EventName}: {description}");

            return fightEvent;
        }
    }
}
