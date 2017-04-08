using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class Character // Includes player characters and non-player characters.
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Dictionary<string, Character> _characters = new Dictionary<string, Character>();

        private Character(string name, string id = null, string organization = null, Profession profession = null)
        {
            Name = name;
            ID = id;
            Organization = organization;
            Profession = profession ?? Profession.Unknown;
        }

        public string Name { get; }
        public string ID { get; set; }
        public Profession Profession { get; }
        public string Organization { get; }

        public static Task<Character[]> GetOrCreateCharacters(IEnumerable<string> names)
            => GetOrCreateCharacters(names.ToArray());

        public static Task<Character[]> GetOrCreateCharacters(params string[] names)
            => Task.WhenAll(names.Select(n => GetOrCreateCharacter(n)).ToArray());

        public static async Task<Character> GetOrCreateCharacter(string name)
        {
            if (_characters.TryGetValue(name, out Character character))
                return character;

            if (!name.Any(Char.IsWhiteSpace) && name.Length > 3 && name.Length < 13) // Could be a PC rather than an NPC then.
            {
                var response = await _httpClient.GetAsync($"http://people.anarchy-online.com/character/bio/d/5/name/{name}/bio.xml?data_type=json");
                if (response.IsSuccessStatusCode)
                {
                    var characterBio = await response.Content.ReadAsAsync<dynamic>();
                    if (characterBio != null) // If the character doesn't exist/hasn't been indexed yet, the JSON returned is null.
                    {
                        var characterInfo = characterBio[0];
                        var organizationInfo = characterBio[1];
                        var profession = Profession.All.Single(p => p.Name == characterInfo.PROF);
                        character = new Character(name, characterInfo.CHAR_INSTANCE, organizationInfo?.NAME, profession);
                    }
                }
            }

            character = character ?? new Character(name);
            _characters.Add(name, character);
            return character;
        }

        public static bool TryGetCharacter(string name, out Character character)
            => _characters.TryGetValue(name, out character);
    }
}
