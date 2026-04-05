using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace AODamageMeter
{
    public class PlayerBioData
    {
        private PlayerBioData() { } // For JSON deserialization.

        public PlayerBioData(string name, Dimension dimension)
        {
            Name = name;
            Dimension = dimension;
        }

        public string Name { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Dimension Dimension { get; private set; }

        public string ID { get; set; }

        [JsonConverter(typeof(ProfessionJsonConverter))]
        public Profession Profession { get; set; } = Profession.Unknown;

        [JsonConverter(typeof(StringEnumConverter))]
        public Breed Breed { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public Faction Faction { get; set; }

        public int? Level { get; set; }
        public int? AlienLevel { get; set; }
        public string Organization { get; set; }
        public string OrganizationRank { get; set; }
        public DateTime? LastUpdatedTimestamp { get; set; }

        [JsonIgnore]
        public bool HasPlayerInfo
            => ID != null
            && Profession != Profession.Unknown
            && Breed != Breed.Unknown
            && Gender != Gender.Unknown
            && Faction != Faction.Unknown
            && Level.HasValue
            && AlienLevel.HasValue;

        [JsonIgnore]
        public bool HasOrganizationInfo
            => Organization != null
            && OrganizationRank != null;

        [JsonIgnore]
        public TimeSpan? Age
            => LastUpdatedTimestamp.HasValue ? DateTime.Now - LastUpdatedTimestamp : null;
    }
}
