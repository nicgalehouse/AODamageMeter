using System;
using System.Linq;

namespace AODamageMeter.UI.Helpers
{
    public static class DamageMeterExtensions
    {
        public static string GetProfessionsInfo(this Fight fight)
            => string.Join(Environment.NewLine, Profession.All
                .Where(fight.HasProfession)
                .Select(p => new
                {
                    profession = p,
                    count = fight.GetProfessionCount(p),
                    averageLevel = fight.GetAverageProfessionLevel(p),
                    averageAlienLevel = fight.GetAverageProfessionAlienLevel(p)
                })
                .Select(a => $"{a.count} {a.profession}{(a.count == 1 ? "" : "s")}, {a.averageLevel.Format()}/{a.averageAlienLevel.Format()}"));

        public static string GetSpecialsInfo(this Fight fight)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => fight.HasDamageTypeDamage(t))
                .Select(t => $@"{fight.GetAverageDamageTypeDamage(t).Value.Format()} dmg"
                    + $", {fight.GetSecondsPerDamageTypeHit(t).Value.Format()} secs / {t.GetName()}"));

        public static string GetSpecialsInfo(this DamageInfo damageInfo)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => damageInfo.HasDamageTypeDamage(t))
                .Select(t => $@"{damageInfo.GetAverageDamageTypeDamage(t).Value.Format()} dmg / {t.GetName()}"));

        public static string GetSpecialsDoneInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => fightCharacter.HasDamageTypeDamageDone(t))
                .Select(t => $@"{fightCharacter.GetAverageDamageTypeDamageDone(t).Value.Format()} dmg"
                    + $", {fightCharacter.GetSecondsPerDamageTypeHitDone(t).Value.Format()} secs / {t.GetName()}"));

        public static string GetSpecialsTakenInfo(this FightCharacter fightCharacter)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => fightCharacter.HasDamageTypeDamageTaken(t))
                .Select(t => $@"{fightCharacter.GetAverageDamageTypeDamageTaken(t).Value.Format()} dmg"
                    + $", {fightCharacter.GetSecondsPerDamageTypeHitTaken(t).Value.Format()} secs / {t.GetName()}"));
    }
}
