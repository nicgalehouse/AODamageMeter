namespace AODamageMeter.UI.Helpers
{
    public static class SharedTooltips
    {
        public static string GetCharacterTooltip(this FightCharacter fightCharacter, int? displayIndex = null)
            => fightCharacter.Character.GetCharacterTooltip(displayIndex);

        public static string GetCharacterTooltip(this Character character, int? displayIndex = null)
=> $"{(displayIndex.HasValue ? $"{displayIndex}. " : "")}{character.UncoloredName}"
+ (!character.HasPlayerInfo ? null : $@"
{character.Level}/{character.AlienLevel} {character.Faction} {character.Profession}
{character.Breed} {character.Gender}")
+ (!character.HasOrganizationInfo ? null : $@"
{character.Organization} ({character.OrganizationRank})");

        public static string GetFightCharacterCountsTooltip(this FightCharacterCounts counts, string title)
=> $@"{title}

{counts.FightCharacterCount} {(counts.FightCharacterCount == 1 ? "character" : "characters")}

{counts.PlayerCount} {(counts.PlayerCount == 1 ? "player" : "players")}, {counts.AveragePlayerLevel.Format()}/{counts.AveragePlayerAlienLevel.Format()}
  {counts.OmniPlayerCount} {(counts.OmniPlayerCount == 1 ? "Omni" : "Omnis")}, {counts.AverageOmniPlayerLevel.Format()}/{counts.AverageOmniPlayerAlienLevel.Format()}
  {counts.ClanPlayerCount} {(counts.ClanPlayerCount == 1 ? "Clan" : "Clan")}, {counts.AverageClanPlayerLevel.Format()}/{counts.AverageClanPlayerAlienLevel.Format()}
  {counts.NeutralPlayerCount} {(counts.NeutralPlayerCount == 1 ? "Neutral" : "Neutrals")}, {counts.AverageNeutralPlayerLevel.Format()}/{counts.AverageNeutralPlayerAlienLevel.Format()}"
+ (counts.UnknownPlayerCount == 0 ? null : $@"
  {counts.UnknownPlayerCount} {(counts.UnknownPlayerCount == 1 ? "Unknown" : "Unknowns")}, {counts.AverageUnknownPlayerLevel.Format()}/{counts.AverageUnknownPlayerAlienLevel.Format()}")
+ (counts.FightPetCount == 0 ? null : $@"
{counts.FightPetCount} {(counts.FightPetCount == 1 ? "pet" : "pets")}")
+ (counts.NPCCount == 0 ? null : $@"
{counts.NPCCount} {(counts.NPCCount == 1 ? "NPC" : "NPCs")}")
+ (counts.PlayerCount == 0 ? null : $@"

{counts.GetProfessionsInfo()}");

        public static string GetFightCharacterDamageDoneTooltip(this FightCharacter fightCharacter,
            string title, int displayIndex, double? percentOfTotal, double? percentOfMax)
=> $@"{displayIndex}. {title}

{fightCharacter.TotalDamageDonePlusPets:N0} total dmg
{percentOfTotal.FormatPercent()} of fight's total dmg
{percentOfMax.FormatPercent()} of fight's max dmg

{fightCharacter.WeaponDamageDonePMPlusPets.Format()} ({fightCharacter.WeaponPercentOfTotalDamageDonePlusPets.FormatPercent()}) weapon dmg / min
{fightCharacter.NanoDamageDonePMPlusPets.Format()} ({fightCharacter.NanoPercentOfTotalDamageDonePlusPets.FormatPercent()}) nano dmg / min
{fightCharacter.IndirectDamageDonePMPlusPets.Format()} ({fightCharacter.IndirectPercentOfTotalDamageDonePlusPets.FormatPercent()}) indirect dmg / min
{fightCharacter.TotalDamageDonePMPlusPets.Format()} total dmg / min

{(fightCharacter.HasIncompleteMissStatsPlusPets ? "≤ " : "")}{fightCharacter.WeaponHitDoneChancePlusPets.FormatPercent()} weapon hit chance
  {fightCharacter.CritDoneChancePlusPets.FormatPercent()} crit chance
  {fightCharacter.GlanceDoneChancePlusPets.FormatPercent()} glance chance

{(fightCharacter.HasIncompleteMissStatsPlusPets ? "≥ " : "")}{fightCharacter.WeaponHitAttemptsDonePMPlusPets.Format()} weapon hit attempts / min
{fightCharacter.WeaponHitsDonePMPlusPets.Format()} weapon hits / min
  {fightCharacter.CritsDonePMPlusPets.Format()} crits / min
  {fightCharacter.GlancesDonePMPlusPets.Format()} glances / min
{fightCharacter.NanoHitsDonePMPlusPets.Format()} nano hits / min
{fightCharacter.IndirectHitsDonePMPlusPets.Format()} indirect hits / min
{fightCharacter.TotalHitsDonePMPlusPets.Format()} total hits / min

{fightCharacter.AverageWeaponDamageDonePlusPets.Format()} weapon dmg / hit
  {fightCharacter.AverageCritDamageDonePlusPets.Format()} crit dmg / hit
  {fightCharacter.AverageGlanceDamageDonePlusPets.Format()} glance dmg / hit
{fightCharacter.AverageNanoDamageDonePlusPets.Format()} nano dmg / hit
{fightCharacter.AverageIndirectDamageDonePlusPets.Format()} indirect dmg / hit"
+ (!fightCharacter.HasSpecialsDone ? null : $@"

{fightCharacter.GetSpecialsDoneInfo()}");

        public static string GetFightCharacterDamageTakenTooltip(this FightCharacter fightCharacter,
            string title, int displayIndex, double? percentOfTotal, double? percentOfMax)
=> $@"{displayIndex}. {title}

{fightCharacter.TotalDamageTaken:N0} total dmg
{percentOfTotal.FormatPercent()} of fight's total dmg
{percentOfMax.FormatPercent()} of fight's max dmg

{fightCharacter.WeaponDamageTakenPM.Format()} ({fightCharacter.WeaponPercentOfTotalDamageTaken.FormatPercent()}) weapon dmg / min
{fightCharacter.NanoDamageTakenPM.Format()} ({fightCharacter.NanoPercentOfTotalDamageTaken.FormatPercent()}) nano dmg / min
{fightCharacter.IndirectDamageTakenPM.Format()} ({fightCharacter.IndirectPercentOfTotalDamageTaken.FormatPercent()}) indirect dmg / min
{fightCharacter.TotalDamageTakenPM.Format()} total dmg / min

{(fightCharacter.HasIncompleteMissStats ? "≤ " : "")}{fightCharacter.WeaponHitTakenChance.FormatPercent()} weapon hit chance
  {fightCharacter.CritTakenChance.FormatPercent()} crit chance
  {fightCharacter.GlanceTakenChance.FormatPercent()} glance chance

{(fightCharacter.HasIncompleteMissStats ? "≤ " : "")}{fightCharacter.WeaponHitAttemptsTakenPM.Format()} weapon hit attempts / min
{fightCharacter.WeaponHitsTakenPM.Format()} weapon hits / min
  {fightCharacter.CritsTakenPM.Format()} crits / min
  {fightCharacter.GlancesTakenPM.Format()} glances / min
{fightCharacter.NanoHitsTakenPM.Format()} nano hits / min
{fightCharacter.IndirectHitsTakenPM.Format()} indirect hits / min
{fightCharacter.TotalHitsTakenPM.Format()} total hits / min

{fightCharacter.AverageWeaponDamageTaken.Format()} weapon dmg / hit
  {fightCharacter.AverageCritDamageTaken.Format()} crit dmg / hit
  {fightCharacter.AverageGlanceDamageTaken.Format()} glance dmg / hit
{fightCharacter.AverageNanoDamageTaken.Format()} nano dmg / hit
{fightCharacter.AverageIndirectDamageTaken.Format()} indirect dmg / hit"
+ (!fightCharacter.HasSpecialsTaken ? null : $@"

{fightCharacter.GetSpecialsTakenInfo()}")
+ (fightCharacter.HitsAbsorbed == 0 ? null : $@"

{fightCharacter.DamageAbsorbed:N0} dmg absorbed
{fightCharacter.HitsAbsorbedPM.Format()} hits absorbed / min
{fightCharacter.DamageAbsorbedPM.Format()} dmg absorbed / min
{fightCharacter.AverageDamageAbsorbed.Format()} dmg absorbed / hit");

        public static string GetOwnersCastsTooltip(this CastInfo castInfo,
            string title, int displayIndex, double? percentOfTotal, double? percentOfMax)
=> $@"{displayIndex}. {title}

{percentOfTotal.FormatPercent()} of total casts
{percentOfMax.FormatPercent()} of max casts

{castInfo.CastSuccesses:N0} ({castInfo.CastSuccessChance.FormatPercent()}) succeeded
{castInfo.CastCountereds:N0} ({castInfo.CastCounteredChance.FormatPercent()}) countered
{castInfo.CastAborteds:N0} ({castInfo.CastAbortedChance.FormatPercent()}) aborted
{castInfo.CastAttempts:N0} attempted

{castInfo.CastSuccessesPM.Format()} succeeded / min
{castInfo.CastCounteredsPM.Format()} countered / min
{castInfo.CastAbortedsPM.Format()} aborted / min
{castInfo.CastAttemptsPM.Format()} attempted / min";

        public static string GetOwnersHealingDoneTooltip(this HealingInfo healingDoneInfo,
            string title, int displayIndex, double? percentOfTotal, double? percentOfMax)
        {
            bool isOwnerTheTarget = healingDoneInfo.Target.IsOwner;

            return
$@"{displayIndex}. {title}

{(isOwnerTheTarget ? "≥ " : "")}{healingDoneInfo.PotentialHealingPlusPets.Format()} potential healing
{percentOfTotal.FormatPercent()} of {healingDoneInfo.Source.UncoloredName}'s total healing
{percentOfMax.FormatPercent()} of {healingDoneInfo.Source.UncoloredName}'s max healing

{(isOwnerTheTarget ? "" : "≥ ")}{healingDoneInfo.RealizedHealingPlusPets.Format()} realized healing
≥ {healingDoneInfo.OverhealingPlusPets.Format()} overhealing
{(isOwnerTheTarget ? "≥ " : "")}{healingDoneInfo.NanoHealingPlusPets.Format()} nano healing

≥ {healingDoneInfo.PercentOfOverhealingPlusPets.FormatPercent()} overhealing";
        }

        public static string GetOwnersHealingTakenTooltip(this HealingInfo healingTakenInfo,
            string title, int displayIndex, double? percentOfTotal, double? percentOfMax)
        {
            bool isOwnerTheSource = healingTakenInfo.Source.IsOwner;

            return
$@"{displayIndex}. {title}

{(isOwnerTheSource ? "≥ " : "")}{healingTakenInfo.PotentialHealingPlusPets.Format()} potential healing
{percentOfTotal.FormatPercent()} of {healingTakenInfo.Target.UncoloredName}'s total healing
{percentOfMax.FormatPercent()} of {healingTakenInfo.Target.UncoloredName}'s max healing

{healingTakenInfo.RealizedHealingPlusPets.Format()} realized healing
{(isOwnerTheSource ? "≥ " : "")}{healingTakenInfo.OverhealingPlusPets.Format()} overhealing
{(isOwnerTheSource ? "≥ " : "")}{healingTakenInfo.NanoHealingPlusPets.Format()} nano healing

{(isOwnerTheSource ? "≥ " : "")}{healingTakenInfo.PercentOfOverhealingPlusPets.FormatPercent()} overhealing";
        }
    }
}
