using AODamageMeter.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class Character
    {
        protected static readonly HttpClient _httpClient = new HttpClient();
        protected static readonly SemaphoreSlim _peopleApiSemaphore = new SemaphoreSlim(3);
        protected static readonly Dictionary<string, Character> _characters = new Dictionary<string, Character>();
        protected static readonly Dictionary<Character, Task> _playerBioRetrievers = new Dictionary<Character, Task>();
        protected volatile PlayerBioData _playerBioData;

        protected Character(string name, Dimension dimension)
        {
            Name = name;
            UncoloredName = NameHelper.UncolorName(name);
            FitsPlayerNamingConventions = NameHelper.FitsPlayerNamingRequirements(name);
            HasAmbiguousPlayerName = NameHelper.IsAmbiguousPlayerName(name);
            FitsPetNamingConventions = NameHelper.TryFitPetNamingConventions(name, out string petMasterName);
            PetMasterName = petMasterName;
            Dimension = dimension;
            IsPlayer = FitsPlayerNamingConventions && !HasAmbiguousPlayerName;
            _playerBioData = new PlayerBioData(name, dimension);
        }

        public string Name { get; }
        public string UncoloredName { get; }
        public bool FitsPlayerNamingConventions { get; }
        public bool HasAmbiguousPlayerName { get; }
        public bool FitsPetNamingConventions { get; }
        public string PetMasterName { get; }
        public Dimension Dimension { get; }
        public bool IsPlayer { get; set; }
        public bool IsNPC => !IsPlayer;

        protected static string GetCharacterKey(string name, Dimension dimension)
            => $"{name} ({dimension.GetName()})";

        public string Key => GetCharacterKey(Name, Dimension);

        public string ID
        {
            get => IsPlayer ? _playerBioData.ID : null;
            set => _playerBioData.ID = value;
        }

        public Profession Profession => IsPlayer ? _playerBioData.Profession : null;
        public Breed? Breed => IsPlayer ? _playerBioData.Breed : (Breed?)null;
        public Gender? Gender => IsPlayer ? _playerBioData.Gender : (Gender?)null;
        public Faction? Faction => IsPlayer ? _playerBioData.Faction : (Faction?)null;
        public int? Level => IsPlayer ? _playerBioData.Level : null;
        public int? AlienLevel => IsPlayer ? _playerBioData.AlienLevel : null;
        public string Organization => IsPlayer ? _playerBioData.Organization : null;
        public string OrganizationRank => IsPlayer ? _playerBioData.OrganizationRank : null;

        public bool HasPlayerInfo => IsPlayer && _playerBioData.HasPlayerInfo;
        public bool HasOrganizationInfo => IsPlayer && _playerBioData.HasOrganizationInfo;

        /* The idea with these two methods is that sometimes you need to wait for the bio to come, sometimes you don't. Provide
         * an overload that exposes the task so it can be awaited if need be. But often it's OK to fire off the bio retriever
         * without awaiting it. Note that we're just using dictionaries, so this method isn't thread-safe. But that's okay, it's
         * only the filling in of the bio after the HTTP request comes in that can happen in parallel. And there we swap the
         * volatile _playerBioData reference atomically, so it's all good. */

        public static Character GetOrCreateCharacter(string name, Dimension dimension)
            => GetOrCreateCharacterAndBioRetriever(name, dimension).character;

        public static (Character character, Task bioRetriever) GetOrCreateCharacterAndBioRetriever(string name, Dimension dimension)
        {
            string characterKey = GetCharacterKey(name, dimension);

            if (_characters.TryGetValue(characterKey, out Character character))
            {
                if (character.FitsPlayerNamingConventions)
                {
                    Task currentPlayerBioRetriever = _playerBioRetrievers[character];

                    if (currentPlayerBioRetriever.IsCompleted && PlayerBioCache.IsStale(character))
                    {
                        currentPlayerBioRetriever = RetrievePlayerBio(character);
                        _playerBioRetrievers[character] = currentPlayerBioRetriever;
                    }

                    return (character, currentPlayerBioRetriever);
                } else return (character, Task.CompletedTask);
            }

            character = new Character(name, dimension);
            _characters.Add(characterKey, character);

            if (character.FitsPlayerNamingConventions)
            {
                Task playerBioRetriever = RetrievePlayerBio(character);
                _playerBioRetrievers.Add(character, playerBioRetriever);
                return (character, playerBioRetriever);
            } else return (character, Task.CompletedTask);
        }

        /* We conclude a name belongs to a player if it fits the player naming requirements and is not ambiguous. A name is ambiguous if
         * it can belong to a player, and definitely does belong to an NPC. For ambiguous names, we assume it's the NPC. We don't require
         * success from people.anarchy-online.com because I want to support new players who aren't indexed yet. Hurts a bit because pet
         * masters can have their pets mistakenly marked as players, but that happens regardless when they name them after players
         * who are already indexed, and that case is even worse. It's also more common because most cool names have been taken by players.
         * If a name is ambiguous we wait for other proof, like from 'me hit by player' or user interaction.

         * The part after the await is safe to be run simultaneously with itself/meter consumers updating their UI--fire and forget this task.
         * Even if we know it won't end up being a player (ambiguous), we still want to get the player info, in case the type is changed later. */

        protected static async Task RetrievePlayerBio(Character character)
        {
            if (PlayerBioCache.TryGet(character, out PlayerBioData cachedPlayerBioData))
            {
                character._playerBioData = cachedPlayerBioData;

                if (!PlayerBioCache.IsStale(character))
                    return;
            }

            await _peopleApiSemaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var peopleApiResponse = await _httpClient
                    .GetAsync($"http://people.anarchy-online.com/character/bio/d/{character.Dimension.GetDimensionID()}/name/{character.Name}/bio.xml?data_type=json")
                    .ConfigureAwait(false);

                if (!peopleApiResponse.IsSuccessStatusCode)
                    return;

                var peopleApiResponseContent = await peopleApiResponse.Content.ReadAsAsync<dynamic>().ConfigureAwait(false);

                if (peopleApiResponseContent == null)
                {
                    // Character probably not indexed yet, update timestamp and preserve any existing data.
                    character._playerBioData.LastUpdatedTimestamp = DateTime.Now;
                }
                else
                {
                    var playerInfo = peopleApiResponseContent[0];
                    var organizationInfo = peopleApiResponseContent[1] as JObject == null ? null : peopleApiResponseContent[1];

                    character._playerBioData = new PlayerBioData(character.Name, character.Dimension)
                    {
                        ID = playerInfo.CHAR_INSTANCE,
                        Profession = Profession.All.Single(p => p.Name == (string)playerInfo.PROF),
                        Breed = BreedHelpers.GetBreed((string)playerInfo.BREED),
                        Gender = GenderHelpers.GetGender((string)playerInfo.SEX),
                        Faction = FactionHelpers.GetFaction((string)playerInfo.SIDE),
                        Level = int.Parse((string)playerInfo.LEVELX),
                        AlienLevel = int.Parse((string)playerInfo.ALIENLEVEL),
                        Organization = organizationInfo?.NAME,
                        OrganizationRank = organizationInfo?.RANK_TITLE,
                        LastUpdatedTimestamp = DateTime.Now
                    };
                }

                PlayerBioCache.AddOrUpdate(character, character._playerBioData);
            }
            catch
            {
                // API failure, update timestamp and preserve any existing data.
                character._playerBioData.LastUpdatedTimestamp = DateTime.Now;
            }
            finally
            {
                _peopleApiSemaphore.Release();
            }
        }

        public static bool TryGetCharacter(string name, Dimension dimension, out Character character)
            => _characters.TryGetValue(GetCharacterKey(name, dimension), out character);


        public override string ToString()
            => UncoloredName;
    }
}
