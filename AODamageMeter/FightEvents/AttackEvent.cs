using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter.FightEvents
{
    public abstract class AttackEvent : FightEvent
    {
        protected AttackEvent(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public FightCharacter Target { get; protected set; }
        public AttackResult AttackResult { get; protected set; }
        public int? Amount { get; protected set; }
        public DamageType? DamageType { get; protected set; }
        public AttackModifier? AttackModifier { get; protected set; }

        protected async Task SetSourceAndTarget(Match match, int sourceIndex, int targetIndex)
        {
            var fightCharacters = await _fight.GetOrCreateFightCharacters(match.Groups[sourceIndex].Value, match.Groups[targetIndex].Value);
            Source = fightCharacters[0];
            Target = fightCharacters[1];
        }

        protected async Task SetTarget(Match match, int index)
            => Target = await _fight.GetOrCreateFightCharacter(match.Groups[index].Value);

        protected void SetSourceAndTargetToOwner()
            => Source = Target = _fight.GetOrCreateFightCharacter(_damageMeter.Owner);

        protected void SetTargetToOwner()
            => Target = _fight.GetOrCreateFightCharacter(_damageMeter.Owner);

        protected void SetAmount(Match match, int index)
            => Amount = int.Parse(match.Groups[index].Value);

        protected void SetDamageType(Match match, int index)
            => DamageType = DamageTypeHelpers.GetDamageType(match.Groups[index].Value);
    }
}
