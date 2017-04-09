using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public abstract class HealEvent : FightEvent
    {
        protected HealEvent(DamageMeter damageMeter, Fight fight, DateTime timestamp, string description)
            : base(damageMeter, fight, timestamp, description)
        { }

        public FightCharacter Target { get; protected set; }
        public int? Amount { get; protected set; }
        public HealType HealType { get; protected set; }

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
    }
}
