using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class Character
    {
        protected static readonly HttpClient _httpClient = new HttpClient();
        protected static readonly HashSet<string> _ambiguousNames = new HashSet<string> { "Abnoba", "Absuum", "Agrama", "Ahomac", "Ahwere", "Akapan", "Alonissos", "Altumus", "Amatsu", "Android", "Aniitap", "Aniuchach", "Aquaan", "Aquarius", "Aquqa", "Arawn", "Aries", "Asanon", "Asatix", "Astypalia", "Atakirh", "Atezac", "Athlar", "Auraka", "Automaton", "Babyface", "Bane", "Banerider", "Bartender", "Beast", "Beit", "Belamorte", "Benacen", "Black", "Bloodcreeper", "Caal", "Cacodemon", "Cancer", "Capricorn", "Cemetiere", "Chiit", "Chirop", "Coah", "Confane", "Cool", "Crete", "Crisp", "Crusher", "Cryptthorn", "Cultist", "Cuty", "Dasyatis", "Degei", "Demenus", "Demon", "Ding", "Direseeds", "Distral", "Duoco", "Edimmu", "Egres", "Ehmat", "Ekibiogami", "Eleet", "Ershkigal", "Esanell", "Eumenides", "Fatestealer", "Fearplanter", "Fearspinner", "Fiend", "Flea", "Floe", "Folegandros", "Forefather", "Galvano", "Gargantula", "Gemini", "George", "Gladiatorbot", "Gluarash", "Goldhorns", "Gravebeau", "Gravejinx", "Grecko", "Grimwolf", "Guard", "Guardbot", "Hatskiri", "Hawilli", "Hawqana", "Haxxor", "Hebiel", "Herne", "Hesosas", "Hine", "Humbada", "Ichiachich", "Igruana", "Ikaria", "Ilmatar", "Inicha", "Ipakut", "Irespouter", "Isham", "Ithaki", "Jealousy", "Jukes", "Jumeaux", "Kalymnos", "Kefallonia", "Keppur", "Khalum", "Kheferu", "Kolaana", "Kumpari", "Kumparimk", "Laki", "Leet", "Leetas", "Lefkada", "Lemur", "Lesvos", "Libra", "Lifebleeder", "Limnos", "Limu", "Maar", "Mare", "Marissa", "Maychwaham", "Maychwyawi", "Maynatsk", "Maywaqaham", "Medinos", "Metalomania", "Milu", "Minibronto", "Minibull", "Minimammoth", "Molokh", "Mornitor", "Morty", "Mull", "Mykonos", "Nabur", "Nanomutant", "Narunkt", "Natsmaahpt", "Naxos", "Nebamun", "Neferu", "Nesbaneb", "Nightcrawler", "Nighthowler", "Numiel", "Nyrckes", "Omathon", "Oscar", "Otacustes", "Ownz", "Ozzus", "Pajainen", "Papagena", "Pareet", "Parspalum", "Paxos", "Penkha", "Phatmos", "Pisces", "Pixie", "Plainrider", "Polly", "Poros", "Powa", "Ptahmose", "Pyiininnik", "Qallyawi", "Ragerider", "Rauni", "Razor", "Restite", "Rockflower", "Rollerrat", "Roxxor", "Sagittarius", "Salahpt", "Saliwata", "Saltworm", "Salvinous", "Samedi", "Sampasa", "Sandcrawler", "Sandworm", "Sanoo", "Sashuqa", "Scorpio", "Sebtitis", "Shadowleet", "Shadowmane", "Shet", "Sifnos", "Silvertails", "Skylight", "Skyros", "Slaugh", "Sleet", "Soleet", "Soulstepper", "Spetses", "Squasher", "Stalker", "Stark", "Stumpy", "Sursunabu", "Swiftwind", "Sylvabane", "Syros", "Takheperu", "Tapiolainen", "Tarasque", "Taurus", "Tcheser", "Tchu", "Tdecin", "Thapt", "Tinos", "Tiny", "Tiunissik", "Tombberry", "Trap", "Trip", "Trup", "Tuaninnik", "Tumulten", "Turk", "Turris", "Uklesh", "Ushamaham", "Vadleany", "Valentyia", "Valirash", "Vilesprout", "Vinstur", "Virgo", "Wakhent", "Waqamawi", "Warbot", "Wardog", "Warmachine", "Wasteprowler", "Wataptu", "Waychaw", "Waywaqa", "Webensenu", "Whire", "Windlegs", "Wrathspouter", "Xark", "Yidira", "Zakynthos", "Zias" };
        protected static readonly Dictionary<string, Character> _characters = new Dictionary<string, Character>();

        protected Character(string name, CharacterType characterType)
        {
            Name = name;
            CharacterType = characterType;
        }

        public string Name { get; }
        public CharacterType CharacterType { get; set; }

        protected string _id;
        public string ID
        {
            get => CharacterType == CharacterType.PlayerCharacter ? ID : null;
            set => _id = value;
        }

        protected Profession _profession;
        public Profession Profession
        {
            get => CharacterType == CharacterType.PlayerCharacter ? (_profession ?? Profession.Unknown) : null;
            set => _profession = value;
        }

        protected string _organization;
        public string Organization
        {
            get => CharacterType == CharacterType.PlayerCharacter ? _organization : null;
            set => _organization = value;
        }

        protected readonly HashSet<Character> _pets = new HashSet<Character>();
        public IReadOnlyCollection<Character> Pets => _pets;
        public void RegisterPet(Character pet)
        {
            if (pet.CharacterType != CharacterType.Pet)
                throw new ArgumentException("A character needs a CharacterType of Pet to be registered as a pet.");
            if (CharacterType != CharacterType.PlayerCharacter)
                throw new ArgumentException("A character needs a CharacterType of PlayerCharacter to own a pet.");

            _pets.Add(pet);
        }

        // Ignoring pets, it's very unlikely that a PC and an NPC of the same name will be participating in the same fight. For more
        // details about pets, see the comment in FightEvent.cs. To summarize, we're able to see when a pet is named the same as the
        // meter owner, and put it under a character named "<Owner>'s pets", so the structure I'm about to describe is flexible enough.
        // The events we have access to would allow us to distinguish between the character named Bloodcreeper doing damage and the mob
        // named Bloodcreeper doing damage in certain scenarios, but not all. It's such a rare occurrence that it's not worth complicating
        // the model, so we have a single dictionary from name to character instead of two (one for PC, one for NPC) or something else.
        // -----------------------------------------------------------------------------------------------------------------------------
        // We conclude a name belongs to a PC if we look it up on people.anarchy-online.com successfully, and it's not ambiguous. A name
        // is ambiguous if it's in the set above--if it can belong to a PC, and definitely does belong to an NPC. For ambiguous names,
        // we assume it's the NPC. The set isn't complete and pets can have random names, so we can't go in the opposite direction--we
        // can't say if it fits the naming requirements and isn't in the ambiguous set, then it belongs to a character. If you're actually
        // playing with the character Bloodcreeper, we'll wait for definitive proof that he's a PC, which only comes through 'me hit by player'.
        // We could use 'other hit by other' where the other is definitely an NPC and assume Bloodcreeper is a PC because NPC v NPC is
        // rare, but that leaves people who rename their pets or bureaucrats in trouble. Could allow setting character type through the UI.
        public static async Task<Character> GetOrCreateCharacter(string name)
        {
            if (_characters.TryGetValue(name, out Character character))
                return character;

            // Even if we know it won't end up being a PC, we still want to get the PC info, in case the type is changed in the future.
            if (FitsPlayerNamingRequirements(name))
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
                        bool isNameAmbiguous = _ambiguousNames.Contains(name);
                        character = new Character(name, isNameAmbiguous ? CharacterType.NonPlayerCharacter : CharacterType.PlayerCharacter)
                        {
                            ID = characterInfo.CHAR_INSTANCE,
                            Profession = profession,
                            Organization = organizationInfo?.NAME
                        };
                    }
                }
            }

            character = character ?? new Character(name, CharacterType.NonPlayerCharacter);
            _characters.Add(name, character);
            return character;
        }

        public static Task<Character[]> GetOrCreateCharacters(params string[] names)
            => Task.WhenAll(names.Select(GetOrCreateCharacter).ToArray());

        public static bool TryGetCharacter(string name, out Character character)
            => _characters.TryGetValue(name, out character);

        public static bool FitsPlayerNamingRequirements(string name)
            => name.Length > 3 && name.Length < 13
            && (name.All(char.IsLetterOrDigit)
                || name.Substring(0, name.Length - 2).All(char.IsLetterOrDigit) && name.EndsWith("-1"));
    }
}
