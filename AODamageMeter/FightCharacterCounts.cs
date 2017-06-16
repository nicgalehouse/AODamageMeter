using AODamageMeter.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter
{
    public class FightCharacterCounts
    {
        protected Dictionary<Profession, int> _professionCounts =  Profession.All.ToDictionary(p => p, p => 0);
        protected Dictionary<Profession, int> _professionTotalLevels = Profession.All.ToDictionary(p => p, p => 0);
        protected Dictionary<Profession, int> _professionTotalAlienLevels = Profession.All.ToDictionary(p => p, p => 0);

        public FightCharacterCounts(Fight fight,
            bool includeNPCs = true,
            bool includeZeroDamageDones = true, bool includeZeroDamageTakens = true,
            bool includeNullOwnersHealingDones = true, bool includeNullOwnersHealingTakens = true)
        {
            Fight = fight;
            fight.TryGetFightOwnerCharacter(out FightCharacter fightOwner);

            foreach (var fightCharacter in fight.FightCharacters
                .Where(c => (includeNPCs || !c.IsNPC)
                    && (includeZeroDamageDones || c.OwnersOrOwnTotalDamageDonePlusPets != 0)
                    && (includeZeroDamageTakens || c.TotalDamageTaken != 0)
                    && (includeNullOwnersHealingDones || (fightOwner?.HealingDoneInfosByTarget.ContainsKey(c) ?? false))
                    && (includeNullOwnersHealingTakens || (fightOwner?.HealingTakenInfosBySource.ContainsKey(c) ?? false))))
            {
                ++FightCharacterCount;
                // We can recognize a character as a player w/o successfully retrieving their bio...
                if (fightCharacter.IsPlayer)
                {
                    ++PlayerCount;
                    // ...So we may not have their level yet, which affects average level calculations down below...
                    if (fightCharacter.Level.HasValue)
                    {
                        ++PlayerWithLevelCount;
                        TotalPlayerLevel += fightCharacter.Level.Value;
                        TotalPlayerAlienLevel += fightCharacter.AlienLevel.Value;
                    }
                }
                // ...But if not-unknown checks down here succeed, it implies we have their bio, including their levels.
                if (fightCharacter.Faction == Faction.Omni)
                {
                    ++OmniPlayerCount;
                    TotalOmniPlayerLevel += fightCharacter.Level.Value;
                    TotalOmniPlayerAlienLevel += fightCharacter.AlienLevel.Value;
                }
                if (fightCharacter.Faction == Faction.Clan)
                {
                    ++ClanPlayerCount;
                    TotalClanPlayerLevel += fightCharacter.Level.Value;
                    TotalClanPlayerAlienLevel += fightCharacter.AlienLevel.Value;
                }
                if (fightCharacter.Faction == Faction.Neutral)
                {
                    ++NeutralPlayerCount;
                    TotalNeutralPlayerLevel += fightCharacter.Level.Value;
                    TotalNeutralPlayerAlienLevel += fightCharacter.AlienLevel.Value;
                }
                if (fightCharacter.Faction == Faction.Unknown) ++UnknownPlayerCount;
                if (fightCharacter.IsPet) ++PetCount;
                if (fightCharacter.IsNPC) ++NPCCount;

                if (fightCharacter.Profession != null)
                {
                    ++_professionCounts[fightCharacter.Profession];

                    if (fightCharacter.Profession != Profession.Unknown)
                    {
                        _professionTotalLevels[fightCharacter.Profession] += fightCharacter.Level.Value;
                        _professionTotalAlienLevels[fightCharacter.Profession] += fightCharacter.AlienLevel.Value;
                    }
                }
            }
        }

        public Fight Fight { get; }
        public int FightCharacterCount { get; }
        public int PlayerCount { get; }
        protected int PlayerWithLevelCount { get; set; }
        public int OmniPlayerCount { get; }
        public int ClanPlayerCount { get; }
        public int NeutralPlayerCount { get; }
        public int UnknownPlayerCount { get; }
        public int PetCount { get; }
        public int NPCCount { get; }
        public int TotalPlayerLevel { get; }
        public int TotalPlayerAlienLevel { get; }
        public int TotalOmniPlayerLevel { get; }
        public int TotalOmniPlayerAlienLevel { get; }
        public int TotalClanPlayerLevel { get; }
        public int TotalClanPlayerAlienLevel { get; }
        public int TotalNeutralPlayerLevel { get; }
        public int TotalNeutralPlayerAlienLevel { get; }
        public int? TotalUnknownPlayerLevel => null;
        public int? TotalUnknownPlayerAlienLevel => null;

        public double? AveragePlayerLevel => TotalPlayerLevel / PlayerWithLevelCount.NullIfZero();
        public double? AveragePlayerAlienLevel => TotalPlayerAlienLevel / PlayerWithLevelCount.NullIfZero();
        public double? AverageOmniPlayerLevel => TotalOmniPlayerLevel / OmniPlayerCount.NullIfZero();
        public double? AverageOmniPlayerAlienLevel => TotalOmniPlayerAlienLevel / OmniPlayerCount.NullIfZero();
        public double? AverageClanPlayerLevel => TotalClanPlayerLevel / ClanPlayerCount.NullIfZero();
        public double? AverageClanPlayerAlienLevel => TotalClanPlayerAlienLevel / ClanPlayerCount.NullIfZero();
        public double? AverageNeutralPlayerLevel => TotalNeutralPlayerLevel / NeutralPlayerCount.NullIfZero();
        public double? AverageNeutralPlayerAlienLevel => TotalNeutralPlayerAlienLevel / NeutralPlayerCount.NullIfZero();
        public double? AverageUnknownPlayerLevel => null;
        public double? AverageUnknownPlayerAlienLevel => null;

        public bool HasProfession(Profession profession) => _professionCounts[profession] != 0;
        public int GetProfessionCount(Profession profession) => _professionCounts[profession];

        public double? GetAverageProfessionLevel(Profession profession)
            => profession == Profession.Unknown ? null : _professionTotalLevels[profession] / _professionCounts[profession].NullIfZero();

        public double? GetAverageProfessionAlienLevel(Profession profession)
            => profession == Profession.Unknown ? null : _professionTotalAlienLevels[profession] / _professionCounts[profession].NullIfZero();
    }
}
