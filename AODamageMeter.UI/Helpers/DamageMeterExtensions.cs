using System;
using System.Linq;

namespace AODamageMeter.UI.Helpers
{
    public static class DamageMeterExtensions
    {
        public static string GetCharacterTooltip(this Character character, int? displayIndex = null)
            => $"{(displayIndex.HasValue ? $"{displayIndex}. " : "")}{character.UncoloredName}"
+ (!character.HasPlayerInfo ? null : $@"
{character.Level}/{character.AlienLevel} {character.Faction} {character.Profession}
{character.Breed} {character.Gender}")
+ (!character.HasOrganizationInfo ? null : $@"
{character.Organization} ({character.OrganizationRank})");

        public static string GetFightCharacterCountsTooltip(this FightCharacterCounts counts)
            => $@"{counts.FightCharacterCount} {(counts.FightCharacterCount == 1 ? "character" : "characters")}
  {counts.PlayerCount} {(counts.PlayerCount == 1 ? "player" : "players")}, {counts.AveragePlayerLevel.Format()}/{counts.AveragePlayerAlienLevel.Format()}
    {counts.OmniPlayerCount} {(counts.OmniPlayerCount == 1 ? "Omni" : "Omnis")}, {counts.AverageOmniPlayerLevel.Format()}/{counts.AverageOmniPlayerAlienLevel.Format()}
    {counts.ClanPlayerCount} {(counts.ClanPlayerCount == 1 ? "Clan" : "Clan")}, {counts.AverageClanPlayerLevel.Format()}/{counts.AverageClanPlayerAlienLevel.Format()}
    {counts.NeutralPlayerCount} {(counts.NeutralPlayerCount == 1 ? "Neutral" : "Neutrals")}, {counts.AverageNeutralPlayerLevel.Format()}/{counts.AverageNeutralPlayerAlienLevel.Format()}"
+ (counts.UnknownPlayerCount == 0 ? null : $@"
    {counts.UnknownPlayerCount} {(counts.UnknownPlayerCount == 1 ? "Unknown" : "Unknowns")}, {counts.AverageUnknownPlayerLevel.Format()}/{counts.AverageUnknownPlayerAlienLevel.Format()}")
+ (counts.PetCount == 0 ? null : $@"
  {counts.PetCount} {(counts.PetCount == 1 ? "pet" : "pets")}")
+ (counts.NPCCount == 0 ? null : $@"
  {counts.NPCCount} {(counts.NPCCount == 1 ? "NPC" : "NPCs")}")
+ (counts.PlayerCount == 0 ? null : $@"

{counts.GetProfessionsInfo()}");

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
