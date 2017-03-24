using Newtonsoft.Json;
using System.Net;

namespace AODamageMeter
{
    public class CharacterBio
    {
        public CharacterInfo CharacterInfo { get; set; }
        public OrgInfo OrgInfo { get; set; }

        public CharacterBio(string characterName)
        {
            try
            {
                string json = new WebClient().DownloadString("http://people.anarchy-online.com/character/bio/d/5/name/" + characterName + "/bio.xml?data_type=json");

                dynamic data = JsonConvert.DeserializeObject<dynamic>(json);

                CharacterInfo = JsonConvert.DeserializeObject<CharacterInfo>(data[0].ToString());

                OrgInfo = JsonConvert.DeserializeObject<OrgInfo>(data[1].ToString());
            }
            catch { }
        }
    }

    public class CharacterInfo
    {
        [JsonProperty("SEX")]
        public string Sex { get; set; }
        [JsonProperty("BREED")]
        public string Breed { get; set; }
        [JsonProperty("PVPRATING")]
        public string PvpRating { get; set; }
        [JsonProperty("NAME")]
        public string Name { get; set; }
        [JsonProperty("FIRSTNAME")]
        public string FirstName { get; set; }
        [JsonProperty("LASTNAME")]
        public string LastName { get; set; }
        [JsonProperty("CHAR_DIMENSION")]
        public string Dimension { get; set; }
        [JsonProperty("ALIENLEVEL")]
        public string AlienLevel { get; set; }
        [JsonProperty("RANK_name")]
        public string RankName { get; set; }
        [JsonProperty("HEADID")]
        public string HeadID { get; set; }
        [JsonProperty("PROFNAME")]
        public string ProfessionTitle { get; set; }
        [JsonProperty("LEVELX")]
        public string Level { get; set; }
        [JsonProperty("PROF")]
        public string Profession { get; set; }
        [JsonProperty("CHAR_INSTANCE")]
        public string CharacterID { get; set; }
        [JsonProperty("SIDE")]
        public string Side { get; set; }
    }

    public class OrgInfo
    {
        [JsonProperty("ORG_DIMENSION")]
        public string Dimension { get; set; }
        [JsonProperty("RANK_TITLE")]
        public string RankTitle { get; set; }
        [JsonProperty("ORG_INSTANCE")]
        public string Instance { get; set; }
        [JsonProperty("NAME")]
        public string Name { get; set; }
        [JsonProperty("RANK")]
        public string Rank { get; set; }
    }
}