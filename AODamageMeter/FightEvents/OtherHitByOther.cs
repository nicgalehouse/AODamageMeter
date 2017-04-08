using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Modifiers = AODamageMeter.Modifier;

namespace AODamageMeter.FightEvents
{
    public class OtherHitByOther : FightEvent
    {
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

        public static async Task<OtherHitByOther> Create(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
        {
            var fightEvent = new OtherHitByOther(damageMeter, fight, timestamp, description);

            bool crit = false, glance = false, reflect = false, shield = false, weirdReflect = false, weirdShield = false, absorb = false;
            if (fightEvent.TryMatch(_normal, out Match match, out bool normal)
                || fightEvent.TryMatch(_crit, out match, out crit)
                || fightEvent.TryMatch(_glance, out match, out glance))
            {
                var fightCharacters = await fight.GetOrCreateFightCharacters(match.Groups[1].Value, match.Groups[2].Value);
                fightEvent.Source = fightCharacters[0];
                fightEvent.Target = fightCharacters[1];
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.Amount = int.Parse(match.Groups[3].Value);
                fightEvent.DamageType = match.Groups[4].Value.GetDamageType();
                fightEvent.Modifier = crit ? Modifiers.Crit
                    : glance ? Modifiers.Glance
                    : (Modifiers?)null;
            }
            else if (fightEvent.TryMatch(_reflect, out match, out reflect)
                || fightEvent.TryMatch(_shield, out match, out shield))
            {
                var fightCharacters = await fight.GetOrCreateFightCharacters(match.Groups[1].Value, match.Groups[2].Value);
                fightEvent.Source = fightCharacters[0];
                fightEvent.Target = fightCharacters[1];
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.Amount = int.Parse(match.Groups[3].Value);
                fightEvent.DamageType = reflect ? DamageType.Reflect : DamageType.Shield;
            }
            else if (fightEvent.TryMatch(_weirdReflect, out match, out weirdReflect)
                || fightEvent.TryMatch(_weirdShield, out match, out weirdShield))
            {
                fightEvent.Target = await fight.GetOrCreateFightCharacter(match.Groups[1].Value);
                fightEvent.ActionType = ActionType.Damage;
                fightEvent.Amount = int.Parse(match.Groups[2].Value);
                fightEvent.DamageType = weirdReflect ? DamageType.Reflect : DamageType.Shield;
            }
            else if (fightEvent.TryMatch(_absorb, out match, out absorb))
            {
                fightEvent.ActionType = ActionType.Absorb;
                fightEvent.Amount = int.Parse(match.Groups[1].Value);
                fightEvent.DamageType = match.Groups[1].Value.GetDamageType();
            }
            else throw new NotSupportedException(description);

            return fightEvent;
        }
    }
}
