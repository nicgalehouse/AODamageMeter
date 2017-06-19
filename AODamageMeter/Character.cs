using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class Character
    {
        protected static readonly HttpClient _httpClient = new HttpClient();
        protected static readonly HashSet<string> _ambiguousPlayerNames = new HashSet<string> { "Abnoba", "Absuum", "Agrama", "Ahomac", "Ahwere", "Akapan", "Alonissos", "Altumus", "Amatsu", "Android", "Aniitap", "Aniuchach", "Aquaan", "Aquarius", "Aquqa", "Arawn", "Aries", "Asanon", "Asatix", "Astypalia", "Atakirh", "Atezac", "Athlar", "Auraka", "Automaton", "Babyface", "Bane", "Banerider", "Bartender", "Beast", "Beit", "Belamorte", "Benacen", "Black", "Bloodcreeper", "Bodyguard", "Bruiser", "Caal", "Cacodemon", "Cancer", "Capricorn", "Cemetiere", "Chiit", "Chirop", "Coah", "Confane", "Cool", "Crete", "Crisp", "Crusher", "Cryptthorn", "Cultist", "Cuty", "Dasyatis", "Degei", "Demenus", "Demon", "Ding", "Direseeds", "Distral", "Dockworker", "Duoco", "Edimmu", "Egres", "Ehmat", "Ekibiogami", "Eleet", "Ershkigal", "Esanell", "Eumenides", "Fatestealer", "Fearplanter", "Fearspinner", "Fiend", "Flea", "Floe", "Folegandros", "Forefather", "Galvano", "Gargantula", "Gemini", "George", "Gladiatorbot", "Gluarash", "Goldhorns", "Gravebeau", "Gravejinx", "Grecko", "Grimwolf", "Guard", "Guardbot", "Gunbeetle", "Hatskiri", "Hawilli", "Hawqana", "Haxxor", "Hebiel", "Herne", "Hesosas", "Hine", "Humbada", "Ichiachich", "Igruana", "Ikaria", "Ilmatar", "Inicha", "Ipakut", "Irespouter", "Isham", "Ithaki", "Jealousy", "Jukes", "Jumeaux", "Kalymnos", "Kefallonia", "Keppur", "Khalum", "Kheferu", "Kolaana", "Kumpari", "Kumparimk", "Laki", "Leet", "Leetas", "Lefkada", "Lemur", "Lesvos", "Libra", "Lifebleeder", "Limnos", "Limu", "Maar", "Mare", "Marissa", "Maychwaham", "Maychwyawi", "Maynatsk", "Maywaqaham", "Medinos", "Metalomania", "Milu", "Minibronto", "Minibull", "Minimammoth", "Molokh", "Mornitor", "Morty", "Mull", "Mykonos", "Nabur", "Nanomutant", "Narunkt", "Natsmaahpt", "Naxos", "Nebamun", "Neferu", "Nesbaneb", "Neutralizer", "Nightcrawler", "Nighthowler", "Numiel", "Nyrckes", "Omathon", "Oscar", "Otacustes", "Ownz", "Ozzus", "Pajainen", "Papagena", "Pareet", "Parspalum", "Paxos", "Penkha", "Phatmos", "Pisces", "Pixie", "Plainrider", "Polly", "Poros", "Powa", "Protester", "Ptahmose", "Pyiininnik", "Qallyawi", "Ragerider", "Rauni", "Razor", "Restite", "Rockflower", "Rollerrat", "Roxxor", "Sagittarius", "Salahpt", "Saliwata", "Saltworm", "Salvinous", "Samedi", "Sampasa", "Sandcrawler", "Sandworm", "Sanoo", "Sashuqa", "Scorpio", "Sebtitis", "Shadowleet", "Shadowmane", "Shet", "Sifnos", "Silvertails", "Skylight", "Skyros", "Slaugh", "Sleet", "Soleet", "Soulstepper", "Spetses", "Squasher", "Stalker", "Stark", "Stumpy", "Suppressor", "Sursunabu", "Swiftwind", "Sylvabane", "Syros", "Takheperu", "Tapiolainen", "Tarasque", "Taurus", "Tcheser", "Tchu", "Tdecin", "Technician", "Thapt", "Tinos", "Tiny", "Tiunissik", "Tombberry", "Trap", "Trip", "Trup", "Tuaninnik", "Tumulten", "Turk", "Turris", "Uklesh", "Ushamaham", "Vadleany", "Valentyia", "Valirash", "Vilesprout", "Vinstur", "Virgo", "Wakhent", "Waqamawi", "Warbot", "Wardog", "Warmachine", "Wasteprowler", "Wataptu", "Waychaw", "Waywaqa", "Webensenu", "Whire", "Windlegs", "Wrathspouter", "Xark", "Yidira", "Zakynthos", "Zias", "Zodiac" };
        protected static readonly HashSet<string> _ambiguousPetNames = new HashSet<string> { "Anansi's Abettor", "Anansi's Disciple", "Anansi's Favorite", "Anansi's Left Hand", "Anansi's Right Hand", "Aniitap's Shadow", "Asase's Drudge", "Nyame's Abettor", "Nyame's Drudge", "Pyiininnik's Shadow", "Sanity's Edge", "Tiunissik's Shadow" };
        protected static readonly Dictionary<string, Character> _characters = new Dictionary<string, Character>();
        protected static readonly Dictionary<Character, Task> _characterBioRetrievers = new Dictionary<Character, Task>();
        protected readonly object _lock = new object(); // Just using a single object for convenience; see comments/links below.

        protected Character(string name)
        {
            Name = name;
            UncoloredName = UncolorName(name);
        }

        public string Name { get; }
        public string UncoloredName { get; }

        protected CharacterType _characterType;
        public CharacterType CharacterType
        {
            get { lock (_lock) { return _characterType; } }
            set { lock (_lock) { _characterType = value; } }
        }

        protected string _id;
        public string ID
        {
            get { lock (_lock) { return IsPlayer ? _id : null; } }
            set { lock (_lock) { _id = value; } }
        }

        protected Profession _profession;
        public Profession Profession
        {
            get { lock (_lock) { return IsPlayer ? (_profession ?? Profession.Unknown) : null; } }
            set { lock (_lock) { _profession = value; } }
        }

        protected Breed? _breed;
        public Breed? Breed
        {
            get { lock (_lock) { return IsPlayer ? (_breed ?? AODamageMeter.Breed.Unknown) : (Breed?)null; } }
            set { lock (_lock) { _breed = value; } }
        }

        protected Gender? _gender;
        public Gender? Gender
        {
            get { lock (_lock) { return IsPlayer ? (_gender ?? AODamageMeter.Gender.Unknown) : (Gender?)null; } }
            set { lock (_lock) { _gender = value; } }
        }

        protected Faction? _faction;
        public Faction? Faction
        {
            get { lock (_lock) { return IsPlayer ? (_faction ?? AODamageMeter.Faction.Unknown) : (Faction?)null; } }
            set { lock (_lock) { _faction = value; } }
        }

        protected int? _level;
        public int? Level
        {
            get { lock (_lock) { return IsPlayer ? _level : null; } }
            set { lock (_lock) { _level = value; } }
        }

        protected int? _alienLevel;
        public int? AlienLevel
        {
            get { lock (_lock) { return IsPlayer ? _alienLevel : null; } }
            set { lock (_lock) { _alienLevel = value; } }
        }

        protected string _organization;
        public string Organization
        {
            get { lock (_lock) { return IsPlayer ? _organization : null; } }
            set { lock (_lock) { _organization = value; } }
        }

        protected string _organizationRank;
        public string OrganizationRank
        {
            get { lock (_lock) { return IsPlayer ? _organizationRank : null; } }
            set { lock (_lock) { _organizationRank = value; } }
        }

        public bool IsPlayer => CharacterType == CharacterType.Player;
        public bool IsNPC => CharacterType == CharacterType.NPC;
        public bool IsPet => CharacterType == CharacterType.Pet;

        public bool HasPlayerInfo
            => IsPlayer
            && ID != null
            && Profession != Profession.Unknown
            && Breed != AODamageMeter.Breed.Unknown
            && Gender != AODamageMeter.Gender.Unknown
            && Faction != AODamageMeter.Faction.Unknown
            && Level.HasValue
            && AlienLevel.HasValue;

        public bool HasOrganizationInfo
            => IsPlayer
            && Organization != null
            && OrganizationRank != null;

        public Character PetOwner { get; protected set; }
        protected readonly HashSet<Character> _pets = new HashSet<Character>();
        public IReadOnlyCollection<Character> Pets => _pets;
        public bool IsPetOwner => Pets.Count != 0;
        public void RegisterPet(Character pet)
        {
            CharacterType = CharacterType.Player;
            pet.CharacterType = CharacterType.Pet;
            _pets.Add(pet);
            pet.PetOwner = this;
        }

        // The idea with these two methods is that sometimes you need to wait for the bio to come, sometimes you don't. Provide
        // an overload that exposes the task so it can be awaited if need be. But often it's OK to fire off the bio retriever
        // without awaiting it. Note that we're just using dictionaries, so this method isn't thread-safe. It's the filling
        // in of the bio, after the HTTP request comes in, that can happen in parallel. When it does happen in parallel we
        // have to worry about locking and volatility and so on, which is why we use locks above. See:
        // http://stackoverflow.com/q/33528408, http://stackoverflow.com/q/434890, http://jonskeet.uk/csharp/threads/volatility.shtml.
        public static Character GetOrCreateCharacter(string name) => GetOrCreateCharacterAndBioRetriever(name).character;
        public static (Character character, Task bioRetriever) GetOrCreateCharacterAndBioRetriever(string name)
        {
            if (_characters.TryGetValue(name, out Character character))
                return (character, _characterBioRetrievers[character]);

            character = new Character(name);
            _characters.Add(name, character);

            Task characterBioRetriever = RetrieveCharacterBio(character);
            _characterBioRetrievers.Add(character, characterBioRetriever);

            return (character, characterBioRetriever);
        }

        public static Character[] GetOrCreateCharacters(params string[] names) => GetOrCreateCharactersAndBioRetrievers(names).characters;
        public static (Character[] characters, Task[] bioRetrievers) GetOrCreateCharactersAndBioRetrievers(params string[] names)
        {
            var characterAndBioRetrievers = names.Select(GetOrCreateCharacterAndBioRetriever).ToArray();
            return (characterAndBioRetrievers.Select(t => t.character).ToArray(), characterAndBioRetrievers.Select(t => t.bioRetriever).ToArray());
        }

        // We conclude a name belongs to a player if it fits the player naming requirements and is not ambiguous. A name is ambiguous if it's
        // in the set above--if it can belong to a player, and definitely does belong to an NPC. For ambiguous names, we assume it's the NPC.
        // We don't require success from people.anarchy-online.com because I want to support new players who aren't indexed yet. Hurts a bit
        // because pet owners can have their pets mistakenly marked as players, but that happens regardless when they name them after players
        // who are already indexed, and that case is even worse. It's also more common because most cool names have been taken by players. If
        // a name is ambiguous we wait for other proof, like from 'me hit by player' or a pet registration.

        // We conclude a name belongs to a pet if it fits this naming convention: "<valid player name>'s <pet name>" (potentially colored),
        // and isn't ambiguous. Pet events aren't very good for deducing pets, and recognizing pets immediately simplifies things (see
        // FightCharacter comment for how). I considered creating the pet owner and registering the pet here too, but seems like bad design.

        // The part after the await is safe to be run simultaneously with itself/meter consumers updating their UI--fire and forget this task.
        // Even if we know it won't end up being a player (ambiguous), we still want to get the player info, in case the type is changed later.
        protected static async Task RetrieveCharacterBio(Character character)
        {
            if (FitsPlayerNamingRequirements(character.Name))
            {
                character.CharacterType = !IsAmbiguousPlayerName(character.Name) ? CharacterType.Player : CharacterType.NPC;

                var response = await _httpClient
                    .GetAsync($"http://people.anarchy-online.com/character/bio/d/5/name/{character.Name}/bio.xml?data_type=json").ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    var characterBio = await response.Content.ReadAsAsync<dynamic>().ConfigureAwait(false);
                    if (characterBio != null) // If the character doesn't exist/hasn't been indexed yet, the JSON returned is null.
                    {
                        var playerInfo = characterBio[0];
                        var organizationInfo = characterBio[1] as JObject == null ? null : characterBio[1];
                        character.ID = playerInfo.CHAR_INSTANCE;
                        character.Profession = Profession.All.Single(p => p.Name == (string)playerInfo.PROF);
                        character.Breed = BreedHelpers.GetBreed((string)playerInfo.BREED);
                        character.Gender = GenderHelpers.GetGender((string)playerInfo.SEX);
                        character.Faction = FactionHelpers.GetFaction((string)playerInfo.SIDE);
                        character.Level = int.Parse((string)playerInfo.LEVELX);
                        character.AlienLevel = int.Parse((string)playerInfo.ALIENLEVEL);
                        character.Organization = organizationInfo?.NAME;
                        character.OrganizationRank = organizationInfo?.RANK_TITLE;
                    }
                }
            }
            else if (FitsPetNamingRequirements(character.Name) && !IsAmbiguousPetName(character.Name))
            {
                character.CharacterType = CharacterType.Pet;
            }
        }

        public static bool TryGetCharacter(string name, out Character character)
            => _characters.TryGetValue(name, out character);

        private static bool IsUppercase(char c) => c >= 'A' && c <= 'Z';
        private static bool IsLowercaseOrDigit(char c) => c >= 'a' && c <= 'z' || c >= '0' && c <= '9';
        public static bool FitsPlayerNamingRequirements(string name)
            => name != null && name.Length > 3 && name.Length < 13
            && IsUppercase(name[0])
            && (name.Skip(1).All(IsLowercaseOrDigit)
                || name.EndsWith("-1") && name.Skip(1).Take(name.Length - 3).All(IsLowercaseOrDigit));

        public static bool FitsPetNamingRequirements(string name)
        {
            int petOwnerNameLength;
            return name != null
                && (petOwnerNameLength = name.IndexOf("'s ")) != -1
                && FitsPlayerNamingRequirements(UncolorName(name.Substring(0, petOwnerNameLength)));
        }

        public static bool TryFitPetNamingRequirements(string name, out string petOwnerName)
        {
            int petOwnerNameLength;
            if (name == null
                || (petOwnerNameLength = name.IndexOf("'s ")) == -1
                || !FitsPlayerNamingRequirements(petOwnerName = UncolorName(name.Substring(0, petOwnerNameLength))))
            {
                petOwnerName = null;
                return false;
            }

            return true;
        }

        public static bool IsAmbiguousPlayerName(string name)
            => _ambiguousPlayerNames.Contains(name);

        public static bool IsAmbiguousPetName(string name)
            => _ambiguousPetNames.Contains(name);

        // Little more involved than this describes: https://forums.anarchy-online.com/showthread.php?587840-Getting-color-back-to-pet-names.
        // I think it's always [DLE][?] that represents colors, but there can be many of those throughout the string for multi-colored names.
        public static string UncolorName(string name)
        {
            char DLE = (char)16;

            if (name.IndexOf(DLE) == -1)
                return name;

            char[] uncoloredName = new char[name.Length];
            int nameIndex = 0, uncoloredNameIndex = 0;
            while (nameIndex < name.Length)
            {
                if (name[nameIndex] == DLE)
                {
                    nameIndex += 2;
                }
                else
                {
                    uncoloredName[uncoloredNameIndex++] = name[nameIndex++];
                }
            }

            return new string(uncoloredName, 0, uncoloredNameIndex);
        }

        public override string ToString()
            => UncoloredName;
    }
}
