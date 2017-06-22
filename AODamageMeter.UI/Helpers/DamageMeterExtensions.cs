using System;
using System.Linq;

namespace AODamageMeter.UI.Helpers
{
    public static class DamageMeterExtensions
    {
        public static string GetProfessionsInfo(this FightCharacterCounts counts)
            => string.Join(Environment.NewLine, Profession.All
                .Where(counts.HasProfession)
                .Select(p => new
                {
                    profession = p,
                    count = counts.GetProfessionCount(p),
                    averageLevel = counts.GetAverageProfessionLevel(p),
                    averageAlienLevel = counts.GetAverageProfessionAlienLevel(p)
                })
                .Select(a => $"{a.count} {a.profession}{(a.count == 1 ? "" : "s")}, {a.averageLevel.Format()}/{a.averageAlienLevel.Format()}"));

        public static string GetSpecialsDoneInfo(this FightDamageDoneStats stats)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => stats.HasDamageTypeDamage(t))
                .Select(t => $@"{stats.GetAverageDamageTypeDamage(t).Value.Format()} dmg"
                    + $", {stats.GetSecondsPerDamageTypeHit(t).Value.Format()} secs / {t.GetName()}"));

        public static string GetSpecialsTakenInfo(this FightDamageTakenStats stats)
            => string.Join(Environment.NewLine, DamageTypeHelpers.SpecialDamageTypes
                .Where(t => stats.HasDamageTypeDamage(t))
                .Select(t => $@"{stats.GetAverageDamageTypeDamage(t).Value.Format()} dmg"
                    + $", {stats.GetSecondsPerDamageTypeHit(t).Value.Format()} secs / {t.GetName()}"));

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

        public static string GetScriptFolderPath(this DamageMeter damageMeter)
            => $"{damageMeter.LogFilePath.Substring(0, damageMeter.LogFilePath.IndexOf("\\Prefs\\"))}\\scripts";

        public static string GetScriptFilePath(this DamageMeter damageMeter)
            => $"{damageMeter.LogFilePath.Substring(0, damageMeter.LogFilePath.IndexOf("\\Prefs\\"))}\\scripts\\aodm";
    }
}
