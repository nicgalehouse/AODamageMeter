using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Modifiers = AODamageMeter.Modifier;

namespace AODamageMeter.FightEvents
{
    public class OtherHitByOther : FightEvent
    {
        public const string EventKey = "0a";
        public const string EventName = "Other hit by other";

        private static readonly Regex
            _normal =       new Regex(@"^(.+?) hit (.+?) for (\d+) points of (.+?) damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _crit =         new Regex(@"^(.+?) hit (.+?) for (\d+) points of (.+?) damage. Critical hit!$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _glance =       new Regex(@"^(.+?) hit (.+?) for (\d+) points of (.+?) damage. Glancing hit.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _reflect =      new Regex(@"^(.+?)'s reflect shield hit (.+?) for (\d+) points of damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _shield =       new Regex(@"^(.+?)'s damage shield hit (.+?) for (\d+) points of damage.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _weirdReflect = new Regex(@"^Something hit (.+?) for (\d+) points of damage by reflect shield.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _weirdShield =  new Regex(@"^Something hit (.+?) for (\d+) points of damage by damage shield.$", RegexOptions.Compiled | RegexOptions.RightToLeft),
            _absorb =       new Regex(@"^Someone absorbed (\d+) points of (.+?) damage.$", RegexOptions.Compiled);

        protected OtherHitByOther(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public override string Key => EventKey;
        public override string Name => EventName;

        public static async Task<OtherHitByOther> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var fightEvent = new OtherHitByOther(damageMeter, fight, timestamp, description);

            bool crit = false, glance = false, reflect = false, shield = false, weirdReflect = false, weirdShield = false, absorb = false;
            if (fightEvent.TryMatch(_normal, out Match match, out bool normal)
                || fightEvent.TryMatch(_crit, out match, out crit)
                || fightEvent.TryMatch(_glance, out match, out glance))
            {
                await fightEvent.SetSourceAndTarget(match, 1, 2);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 3);
                fightEvent.SetDamageType(match, 4);
                fightEvent.Modifier = crit ? Modifiers.Crit
                    : glance ? Modifiers.Glance
                    : (Modifiers?)null;
            }
            else if (fightEvent.TryMatch(_reflect, out match, out reflect)
                || fightEvent.TryMatch(_shield, out match, out shield))
            {
                await fightEvent.SetSourceAndTarget(match, 1, 2);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 3);
                fightEvent.DamageType = reflect ? DamageType.Reflect : DamageType.Shield;
            }
            else if (fightEvent.TryMatch(_weirdReflect, out match, out weirdReflect)
                || fightEvent.TryMatch(_weirdShield, out match, out weirdShield))
            {
                await fightEvent.SetTarget(match, 1);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.SetAmount(match, 2);
                fightEvent.DamageType = weirdReflect ? DamageType.Reflect : DamageType.Shield;
            }
            else if (fightEvent.TryMatch(_absorb, out match, out absorb))
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
