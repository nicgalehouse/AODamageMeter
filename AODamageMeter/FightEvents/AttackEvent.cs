using System;
using System.Text.RegularExpressions;

namespace AODamageMeter.FightEvents
{
    public abstract class AttackEvent : FightEvent
    {
        protected const string DAMAGETYPE = "(.+?)";

        protected AttackEvent(Fight fight, DateTime timestamp, string description)
            : base(fight, timestamp, description)
        { }

        public AttackResult AttackResult { get; protected set; }
        public DamageType? DamageType { get; protected set; }
        public AttackModifier? AttackModifier { get; protected set; }
        public bool IsPVP => Source.IsPlayer && Target.IsPlayer;
        public bool IsSpecialDamage => DamageType?.IsSpecialDamageType() ?? false;

        protected void SetDamageType(Match match, int index)
            => DamageType = DamageTypeHelpers.GetDamageType(match.Groups[index].Value);
    }
}
