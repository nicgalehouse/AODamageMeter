using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AODamageMeter
{
    public class Character
    {
        protected static readonly HttpClient _httpClient = new HttpClient();
        protected static readonly HashSet<string> _ambiguousNames = new HashSet<string> { "Abnoba", "Absuum", "Agrama", "Ahomac", "Ahwere", "Akapan", "Alonissos", "Altumus", "Amatsu", "Android", "Aniitap", "Aniuchach", "Aquaan", "Aquarius", "Aquqa", "Arawn", "Aries", "Asanon", "Asatix", "Astypalia", "Atakirh", "Atezac", "Athlar", "Auraka", "Automaton", "Babyface", "Bane", "Banerider", "Bartender", "Beast", "Beit", "Belamorte", "Benacen", "Black", "Bloodcreeper", "Caal", "Cacodemon", "Cancer", "Capricorn", "Cemetiere", "Chiit", "Chirop", "Coah", "Confane", "Cool", "Crete", "Crisp", "Crusher", "Cryptthorn", "Cultist", "Cuty", "Dasyatis", "Degei", "Demenus", "Demon", "Ding", "Direseeds", "Distral", "Duoco", "Edimmu", "Egres", "Ehmat", "Ekibiogami", "Eleet", "Ershkigal", "Esanell", "Eumenides", "Fatestealer", "Fearplanter", "Fearspinner", "Fiend", "Flea", "Floe", "Folegandros", "Forefather", "Galvano", "Gargantula", "Gemini", "George", "Gladiatorbot", "Gluarash", "Goldhorns", "Gravebeau", "Gravejinx", "Grecko", "Grimwolf", "Guard", "Guardbot", "Hatskiri", "Hawilli", "Hawqana", "Haxxor", "Hebiel", "Herne", "Hesosas", "Hine", "Humbada", "Ichiachich", "Igruana", "Ikaria", "Ilmatar", "Inicha", "Ipakut", "Irespouter", "Isham", "Ithaki", "Jealousy", "Jukes", "Jumeaux", "Kalymnos", "Kefallonia", "Keppur", "Khalum", "Kheferu", "Kolaana", "Kumpari", "Kumparimk", "Laki", "Leet", "Leetas", "Lefkada", "Lemur", "Leo", "Lesvos", "Libra", "Lifebleeder", "Limnos", "Limu", "Maar", "Mare", "Marissa", "Maychwaham", "Maychwyawi", "Maynatsk", "Maywaqaham", "Medinos", "Metalomania", "Milu", "Minibronto", "Minibull", "Minimammoth", "Molokh", "Mornitor", "Morty", "Mull", "Mykonos", "Nabur", "Nanomutant", "Narunkt", "Natsmaahpt", "Naxos", "Nebamun", "Neferu", "Nesbaneb", "Nightcrawler", "Nighthowler", "Numiel", "Nyrckes", "Omathon", "Oscar", "Otacustes", "Ownz", "Ozzus", "Pajainen", "Papagena", "Pareet", "Parspalum", "Paxos", "Penkha", "Phatmos", "Pisces", "Pixie", "Plainrider", "Polly", "Poros", "Powa", "Ptahmose", "Pyiininnik", "Qallyawi", "Ragerider", "Rauni", "Razor", "Restite", "Rockflower", "Rollerrat", "Roxxor", "Sagittarius", "Salahpt", "Saliwata", "Saltworm", "Salvinous", "Samedi", "Sampasa", "Sandcrawler", "Sandworm", "Sanoo", "Sashuqa", "Scorpio", "Sebtitis", "Shadowleet", "Shadowmane", "Shet", "Sifnos", "Silvertails", "Skylight", "Skyros", "Slaugh", "Sleet", "Soleet", "Soulstepper", "Spetses", "Squasher", "Stalker", "Stark", "Stumpy", "Sursunabu", "Swiftwind", "Sylvabane", "Syros", "Takheperu", "Tapiolainen", "Tarasque", "Taurus", "Tcheser", "Tchu", "Tdecin", "Thanatosflower", "Thapt", "Tinos", "Tiny", "Tiunissik", "Tombberry", "Trap", "Trip", "Trup", "Tuaninnik", "Tumulten", "Turk", "Turris", "Uklesh", "Ushamaham", "Vadleany", "Valentyia", "Valirash", "Vilesprout", "Vinstur", "Virgo", "Wakhent", "Waqamawi", "Warbot", "Wardog", "Warmachine", "Wasteprowler", "Wataptu", "Waychaw", "Waywaqa", "Webensenu", "Whire", "Windlegs", "Wrathspouter", "Xark", "Yidira", "Zakynthos", "Zias" };
        protected static readonly Dictionary<string, Character> _characters = new Dictionary<string, Character>();

        protected Character(string name, CharacterType characterType)
        {
            Name = name;
            IsNameAmbiguous = _ambiguousNames.Contains(Name);
            CharacterType = characterType;
        }

        public string Name { get; }
        public bool IsNameAmbiguous { get; }
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

        public static Task<Character[]> GetOrCreateCharacters(IEnumerable<string> names, CharacterType? knownCharacterType = null)
            => Task.WhenAll(names.Select(n => GetOrCreateCharacter(n, knownCharacterType)).ToArray());

        // Ignoring pets, it's very unlikely that a PC and an NPC of the same name will be participating in the same fight. In the case
        // of pets, we don't really have a way to distinguish them from a PC unless they're the meter owner's pets. And even then without
        // some ad hoc, approximate work, we can only correlate the pet back to the owner if it has the same name. The case where the
        // meter owner has pets named after himself can be handled higher up, by creating a character w/ a name like "Owner's pet". The
        // case where anyone else has pets isn't worried about--if they name the pet the same as themselves, they'll get their pet's DPS,
        // but we won't be able to break down the DPS into individual buckets for the owner and the pet. But back to the original point,
        // the events we have access to would allow us to distinguish between the character named Bloodcreeper doing damage and the mob
        // named Bloodcreeper doing damage in certain scenarios, but not all. It's such a rare occurrence that it's not worth complicating
        // the model, so we have a single dictionary from name to character instead of two (one for PC, one for NPC) or something else.
        // -----------------------------------------------------------------------------------------------------------------------------
        // We conclude a name belongs to a PC if we look it up on people.anarchy-online.com successfully, and it's not ambiguous. A name
        // is ambiguous if it's in the set above--if it can belong to a PC, and definitely does belong to an NPC. For ambiguous names,
        // we assume it's the NPC. The set isn't complete and pets can have random names, so we can't go in the opposite direction--we
        // can't say if it fits the naming requirements and isn't in the ambiguous set, then it belongs to a character. If you're actually
        // playing with the character Bloodcreeper, we'll wait for definitive proof that he's a PC, which only comes through 'me hit by player'.
        // We could use 'other hit by other' where the other is definitely an NPC and assume Bloodcreeper is a PC because NPC v NPC is
        // rare, but that leaves people who rename their pets or bureaucrats in trouble. Most people who've taken the cool NPC names
        // don't play anymore took them years ago and don't play anymore, and the uncool NPC names probably aren't taken anyway. That
        // being said, all the properties are settable to allow human interaction for setting more accurate values than we/PoRK have.
        public static async Task<Character> GetOrCreateCharacter(string name, CharacterType? knownCharacterType = null)
        {
            if (_characters.TryGetValue(name, out Character character))
            {
                character.CharacterType = knownCharacterType ?? character.CharacterType;
                return character;
            }

            // Even if we know it won't end up being a PC, we still want to get the PC info, in case the type is changed in the future.
            if (name.All(char.IsLetterOrDigit) && name.Length > 3 && name.Length < 13)
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
                        character = new Character(name,
                            knownCharacterType ?? (isNameAmbiguous ? CharacterType.NonPlayerCharacter : CharacterType.PlayerCharacter))
                        {
                            ID = characterInfo.CHAR_INSTANCE,
                            Profession = profession,
                            Organization = organizationInfo?.NAME
                        };
                    }
                }
            }

            character = character ?? new Character(name, knownCharacterType ?? CharacterType.NonPlayerCharacter);
            _characters.Add(name, character);
            return character;
        }
    }
}
