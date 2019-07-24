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
        protected static readonly HashSet<string> _ambiguousPlayerNames = new HashSet<string> { "Abnoba", "Absuum", "Agrama", "Ahomac", "Ahpta", "Ahwere", "Akapan", "Alonissos", "Altumus", "Amatsu", "Android", "Aniitap", "Aniuchach", "Aquaan", "Aquarius", "Aquqa", "Arachnis", "Arachnobot", "Arawn", "Arcorash", "Aries", "Asanon", "Asatix", "Assembler", "Astypalia", "Atakirh", "Atezac", "Athlar", "Auraka", "Automaton", "Babyface", "Bane", "Banerider", "Bartender", "Beast", "Beit", "Belamorte", "Benacen", "Black", "Bloodcreeper", "Blorrg", "Bodyguard", "Bruiser", "Caal", "Cacodemon", "Cancer", "Capricorn", "Cemetiere", "Chiit", "Chirop", "Chontamenti", "Coah", "Collector", "Confane", "Cool", "Creepos", "Crete", "Crisp", "Crusher", "Cryptthorn", "Cultist", "Cuty", "Dasyatis", "Degei", "Demenus", "Demisedope", "Demon", "Ding", "Direseeds", "Distral", "Dockworker", "Doombringer", "Duoco", "Eclipser", "Edimmu", "Egres", "Ehmat", "Ekibiogami", "Eleet", "Eradicus", "Ercorash", "Ershkigal", "Esanell", "Eumenides", "Fanatic", "Fatestealer", "Fearplanter", "Fearspinner", "Fiend", "Flea", "Floe", "Folegandros", "Forefather", "Frother", "Galvano", "Gargantula", "Gemini", "George", "Gladiatorbot", "Gluarash", "Goldhorns", "Gravebeau", "Gravejinx", "Grecko", "Grimwolf", "Guard", "Guardbot", "Gunbeetle", "Haqa", "Hatskiri", "Hawilli", "Hawqana", "Haxxor", "Hebiel", "Hellhound", "Herne", "Hesosas", "Hiathlin", "Hine", "Hoathlan", "Humbada", "Hwall", "Ichiachich", "Igruana", "Ikaria", "Ilmatar", "Infector", "Inicha", "Ipakut", "Irespouter", "Isham", "Ithaki", "Jealousy", "Jikininki", "Jukes", "Jumeaux", "Kalymnos", "Kefallonia", "Keppur", "Khalum", "Kheferu", "Kizzermole", "Kolaana", "Kumpari", "Kumparimk", "Laki", "Leet", "Leetas", "Lefkada", "Lemur", "Lesvos", "Libra", "Lifebleeder", "Limnos", "Limu", "Looter", "Maar", "Mare", "Marissa", "Maychwaham", "Maychwyawi", "Maynatsk", "Maywaqaham", "Medinos", "Metalomania", "Milu", "Minibronto", "Minibull", "Minimammoth", "Molokh", "Mornitor", "Morty", "Mugger", "Mull", "Mykonos", "Nabur", "Nanomutant", "Narunkt", "Natsmaahpt", "Naxos", "Nebamun", "Necromancer", "Neferu", "Nesbaneb", "Neutralizer", "Nightcrawler", "Nighthowler", "Numiel", "Nyrckes", "Odqan", "Omathon", "Ormazd", "Oscar", "Otacustes", "Ownz", "Ozzus", "Pajainen", "Papagena", "Pareet", "Parspalum", "Paxos", "Penkha", "Phatmos", "Pisces", "Pixie", "Plainrider", "Polly", "Poros", "Powa", "Protester", "Ptahmose", "Pyiininnik", "Python", "Qallyawi", "Ragerider", "Rashk", "Rashnu", "Rauni", "Razor", "Restite", "Rockflower", "Rollerrat", "Roxxor", "Sagittarius", "Salahpt", "Saliwata", "Saltworm", "Salvinous", "Samedi", "Sampasa", "Sandcrawler", "Sandworm", "Sanoo", "Sashuqa", "Scorchid", "Scorpio", "Sebtitis", "Shadow", "Shadowleet", "Shadowmane", "Shet", "Sifnos", "Silvertails", "Skincrawler", "Skylight", "Skyros", "Slaugh", "Sleet", "Soleet", "Somphos", "Soulstepper", "Spetses", "Squasher", "Sraosha", "Stalker", "Stark", "Stinger", "Stumpy", "Suppressor", "Sursunabu", "Swiftwind", "Sylvabane", "Syros", "Takheperu", "Tapiolainen", "Tarasque", "Taurus", "Tcheser", "Tchu", "Tdecin", "Technician", "Tentacle", "Thapt", "Thief", "Tinos", "Tiny", "Tiunissik", "Tombberry", "Trap", "Trip", "Trup", "Tuaninnik", "Tumulten", "Turk", "Turris", "Uklesh", "Unionist", "Ushamaham", "Ushqa", "Vadleany", "Vahman", "Valentyia", "Valirash", "Vicar", "Vilesprout", "Vinstur", "Virgo", "Vizaresh", "Voidling", "Vortexoid", "Wakhent", "Waqamawi", "Warbot", "Wardog", "Warmachine", "Wasteprowler", "Wataptu", "Waychaw", "Waywaqa", "Webensenu", "Whiro", "Windlegs", "Wrathspouter", "Xark", "Xeshm", "Yidira", "Zakynthos", "Zias", "Zodiac" };
        protected static readonly HashSet<string> _ambiguousPetNames = new HashSet<string> { "Anansi's Abettor", "Anansi's Acolyte", "Anansi's Child", "Anansi's Cosset", "Anansi's Disciple", "Anansi's Favorite", "Anansi's Left Hand", "Anansi's Right Hand", "Anansi's Seer", "Anansi's Warden", "Aniitap's Shadow", "Asase's Child", "Asase's Drudge", "Asase's Guardian", "Cerubin's Lazy Tentacle", "Cerubin's Tentacle of Burning Truth", "Cerubin's Tentacle of Cure", "Cerubin's Tentacle of Shock", "Ghasap's Minion", "Nyame's Abettor", "Nyame's Drudge", "Pyiininnik's Shadow", "Sanity's Edge", "Smug's Control Tower", "Tiunissik's Shadow" };
        protected static readonly Dictionary<string, Character> _characters = new Dictionary<string, Character>();
        protected static readonly Dictionary<Character, Task> _characterBioRetrievers = new Dictionary<Character, Task>();
        protected readonly object _lock = new object(); // Just using a single object for convenience; see comments/links below.

        protected Character(string name, Dimension dimension)
        {
            Name = name;
            UncoloredName = UncolorName(name);
            Dimension = dimension;
        }

        public string Name { get; }
        public string UncoloredName { get; }

        public Dimension Dimension { get; }

        protected bool _isPlayer;
        public bool IsPlayer
        {
            get { lock (_lock) { return _isPlayer; } }
            set { lock (_lock) { _isPlayer = value; } }
        }

        public bool IsNPC => !IsPlayer;

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

        // The idea with these two methods is that sometimes you need to wait for the bio to come, sometimes you don't. Provide
        // an overload that exposes the task so it can be awaited if need be. But often it's OK to fire off the bio retriever
        // without awaiting it. Note that we're just using dictionaries, so this method isn't thread-safe. It's the filling
        // in of the bio, after the HTTP request comes in, that can happen in parallel. When it does happen in parallel we
        // have to worry about locking and volatility and so on, which is why we use locks above. See:
        // http://stackoverflow.com/q/33528408, http://stackoverflow.com/q/434890, http://jonskeet.uk/csharp/threads/volatility.shtml.
        public static Character GetOrCreateCharacter(string name, Dimension dimension)
            => GetOrCreateCharacterAndBioRetriever(name, dimension).character;
        public static (Character character, Task bioRetriever) GetOrCreateCharacterAndBioRetriever(string name, Dimension dimension)
        {
            if (_characters.TryGetValue($"{name} ({dimension.GetName()})", out Character character))
                return (character, _characterBioRetrievers[character]);

            character = new Character(name, dimension);
            _characters.Add($"{name} ({dimension.GetName()})", character);

            Task characterBioRetriever = RetrieveCharacterBio(character);
            _characterBioRetrievers.Add(character, characterBioRetriever);

            return (character, characterBioRetriever);
        }

        // We conclude a name belongs to a player if it fits the player naming requirements and is not ambiguous. A name is ambiguous if it's
        // in the set above--if it can belong to a player, and definitely does belong to an NPC. For ambiguous names, we assume it's the NPC.
        // We don't require success from people.anarchy-online.com because I want to support new players who aren't indexed yet. Hurts a bit
        // because pet masters can have their pets mistakenly marked as players, but that happens regardless when they name them after players
        // who are already indexed, and that case is even worse. It's also more common because most cool names have been taken by players. If
        // a name is ambiguous we wait for other proof, like from 'me hit by player' or user interaction.

        // The part after the await is safe to be run simultaneously with itself/meter consumers updating their UI--fire and forget this task.
        // Even if we know it won't end up being a player (ambiguous), we still want to get the player info, in case the type is changed later.
        protected static async Task RetrieveCharacterBio(Character character)
        {
            if (FitsPlayerNamingRequirements(character.Name))
            {
                character.IsPlayer = !IsAmbiguousPlayerName(character.Name);

                var response = await _httpClient
                    .GetAsync($"http://people.anarchy-online.com/character/bio/d/{character.Dimension.GetDimensionID()}/name/{character.Name}/bio.xml?data_type=json")
                    .ConfigureAwait(false);
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
        }

        public static bool TryGetCharacter(string name, Dimension dimension, out Character character)
            => _characters.TryGetValue($"{name} ({dimension.GetName()})", out character);

        private static bool IsUppercase(char c) => c >= 'A' && c <= 'Z';
        private static bool IsLowercaseOrDigit(char c) => c >= 'a' && c <= 'z' || c >= '0' && c <= '9';
        public static bool FitsPlayerNamingRequirements(string name)
            => name != null && name.Length > 3 && name.Length < 13
            && IsUppercase(name[0])
            && (name.Skip(1).All(IsLowercaseOrDigit)
                || name.EndsWith("-1") && name.Skip(1).Take(name.Length - 3).All(IsLowercaseOrDigit));

        public bool FitsPetNamingConventions() => FitsPetNamingConventions(Name);
        public static bool FitsPetNamingConventions(string name)
        {
            int petMasterNameLength;
            return name != null
                && (petMasterNameLength = name.IndexOf("'s ")) != -1
                && FitsPlayerNamingRequirements(UncolorName(name.Substring(0, petMasterNameLength)));
        }

        public bool TryFitPetNamingConventions(out string petMasterName) => TryFitPetNamingConventions(Name, out petMasterName);
        public static bool TryFitPetNamingConventions(string name, out string petMasterName)
        {
            int petMasterNameLength;
            if (name == null
                || (petMasterNameLength = name.IndexOf("'s ")) == -1
                || !FitsPlayerNamingRequirements(petMasterName = UncolorName(name.Substring(0, petMasterNameLength))))
            {
                petMasterName = null;
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
