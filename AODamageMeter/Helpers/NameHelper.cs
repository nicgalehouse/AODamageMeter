using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter.Helpers
{
    public static class NameHelper
    {
        private static readonly HashSet<string> _ambiguousPlayerNames = new HashSet<string> { "Abnoba", "Absuum", "Agrama", "Ahomac", "Ahpta", "Ahwere", "Akapan", "Alonissos", "Altumus", "Amatsu", "Android", "Aniitap", "Aniuchach", "Anvian", "Aquaan", "Aquarius", "Aquqa", "Arachnis", "Arachnobot", "Arawn", "Arcorash", "Aries", "Asanon", "Asatix", "Assembler", "Astypalia", "Atakirh", "Atezac", "Athlar", "Auraka", "Automaton", "Babyface", "Bane", "Banerider", "Bartender", "Beast", "Beit", "Belamorte", "Benacen", "Biledrinker", "Black", "Bloodcreeper", "Blorrg", "Bodyguard", "Bruiser", "Caal", "Cacodemon", "Cancer", "Capricorn", "Cemetiere", "Chiit", "Chirop", "Chontamenti", "Coah", "Collector", "Confane", "Cool", "Creepos", "Crete", "Crisp", "Crusher", "Cryptthorn", "Cultist", "Cuty", "Dasyatis", "Degei", "Demenus", "Demisedope", "Demon", "Ding", "Direseeds", "Distral", "Dockworker", "Doombringer", "Duoco", "Eclipser", "Edimmu", "Egres", "Ehmat", "Ekibiogami", "Eleet", "Eradicus", "Ercorash", "Ershkigal", "Esanell", "Eumenides", "Fanatic", "Fatestealer", "Fearplanter", "Fearspinner", "Fiend", "Flea", "Floe", "Folegandros", "Forefather", "Frother", "Galvano", "Gargantula", "Gemini", "George", "Gladiatorbot", "Gluarash", "Goldhorns", "Gravebeau", "Gravejinx", "Grecko", "Grimwolf", "Guard", "Guardbot", "Gunbeetle", "Haqa", "Hatskiri", "Havaris", "Hawilli", "Hawqana", "Haxxor", "Hebiel", "Hellhound", "Herne", "Hesosas", "Hiathlin", "Hine", "Hoathlan", "Humbada", "Hwall", "Ichiachich", "Igruana", "Ikaria", "Ilmatar", "Infector", "Inicha", "Ipakut", "Irespouter", "Isham", "Ithaki", "Jealousy", "Jikininki", "Jukes", "Jumeaux", "Kalymnos", "Kefallonia", "Keppur", "Khalum", "Kheferu", "Kizzermole", "Kolaana", "Kumpari", "Kumparimk", "Laki", "Leet", "Leetas", "Lefkada", "Lemur", "Lesvos", "Libra", "Lifebleeder", "Limnos", "Limu", "Looter", "Maar", "Malah", "Mare", "Marissa", "Maychwaham", "Maychwyawi", "Maynatsk", "Maywaqaham", "Medinos", "Metalomania", "Milu", "Minibronto", "Minibull", "Minimammoth", "Molokh", "Mornitor", "Morty", "Mugger", "Mull", "Mykonos", "Nabur", "Nanomutant", "Nanovoider", "Narunkt", "Natsmaahpt", "Naxos", "Nebamun", "Necromancer", "Neferu", "Nesbaneb", "Neutralizer", "Nightcrawler", "Nighthowler", "Numiel", "Nyrckes", "Odqan", "Omathon", "Ormazd", "Oscar", "Ossuz", "Otacustes", "Ownz", "Ozzus", "Pajainen", "Papagena", "Pareet", "Parspalum", "Paxos", "Penkha", "Phatmos", "Pisces", "Pixie", "Plainrider", "Plainscale", "Polly", "Poros", "Powa", "Protester", "Ptahmose", "Pyiininnik", "Python", "Qallyawi", "Ragerider", "Rashk", "Rashnu", "Rauni", "Razor", "Restite", "Rockflower", "Rollerrat", "Roxxor", "Sagittarius", "Salahpt", "Saliwata", "Saltworm", "Salvinous", "Samedi", "Sampasa", "Sandcrawler", "Sandworm", "Sanoo", "Sashuqa", "Scorchid", "Scorpio", "Sebtitis", "Shadow", "Shadowleet", "Shadowmane", "Shet", "Sifnos", "Silvertails", "Skincrawler", "Skylight", "Skyros", "Slaugh", "Sleet", "Slimebather", "Snaamehiel", "Soleet", "Somphos", "Soulstepper", "Spetses", "Squasher", "Sraosha", "Stalker", "Stark", "Stinger", "Stumpy", "Suininnik", "Suppressor", "Sursunabu", "Swiftwind", "Sylvabane", "Syros", "Takheperu", "Tapiolainen", "Tarasque", "Taurus", "Tcheser", "Tchu", "Tdecin", "Technician", "Tentacle", "Thapt", "Thief", "Tinos", "Tiny", "Tiunissik", "Tombberry", "Trap", "Trip", "Trup", "Tuaninnik", "Tumulten", "Turk", "Turris", "Uklesh", "Unionist", "Ushamaham", "Ushqa", "Vadleany", "Vahman", "Valentyia", "Valirash", "Vicar", "Vilesprout", "Vinstur", "Virgo", "Vizaresh", "Voidling", "Vortexoid", "Wakhent", "Waqamawi", "Warbot", "Wardog", "Warmachine", "Wasteprowler", "Wataptu", "Waychaw", "Waywaqa", "Webensenu", "Whiro", "Windlegs", "Wrathspouter", "Xark", "Xeshm", "Yidira", "Zakynthos", "Zias", "Zodiac" };
        private static readonly HashSet<string> _ambiguousPetNames = new HashSet<string> { "Anansi's Abettor", "Anansi's Acolyte", "Anansi's Child", "Anansi's Cosset", "Anansi's Disciple", "Anansi's Favorite", "Anansi's Left Hand", "Anansi's Right Hand", "Anansi's Seer", "Anansi's Warden", "Aniitap's Shadow", "Asase's Child", "Asase's Drudge", "Asase's Guardian", "Cerubin's Lazy Tentacle", "Cerubin's Tentacle of Burning Truth", "Cerubin's Tentacle of Cure", "Cerubin's Tentacle of Shock", "Ghasap's Minion", "Nyame's Abettor", "Nyame's Drudge", "Pyiininnik's Shadow", "Sanity's Edge", "Smug's Control Tower", "Tiunissik's Shadow" };

        private static bool IsUppercase(char c) => c >= 'A' && c <= 'Z';
        private static bool IsLowercaseOrDigit(char c) => c >= 'a' && c <= 'z' || c >= '0' && c <= '9';

        public static bool FitsPlayerNamingRequirements(string name)
            => name != null && name.Length > 3 && name.Length < 13
            && IsUppercase(name[0])
            && (name.Skip(1).All(IsLowercaseOrDigit)
                || name.EndsWith("-1") && name.Skip(1).Take(name.Length - 3).All(IsLowercaseOrDigit));

        public static bool FitsPetNamingConventions(string name)
        {
            int petMasterNameLength;
            return name != null
                && (petMasterNameLength = name.IndexOf("'s ")) != -1
                && FitsPlayerNamingRequirements(UncolorName(name.Substring(0, petMasterNameLength)));
        }

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
    }
}
