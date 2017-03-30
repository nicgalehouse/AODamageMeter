using AODamageMeter.Professions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;

namespace AODamageMeter
{
    public abstract class Profession
    {
        public static readonly Adventurer Adventurer = new Adventurer();
        public static readonly Agent Agent = new Agent();
        public static readonly Bureaucrat Bureaucrat = new Bureaucrat();
        public static readonly Doctor Doctor = new Doctor();
        public static readonly Enforcer Enforcer = new Enforcer();
        public static readonly Engineer Engineer = new Engineer();
        public static readonly Fixer Fixer = new Fixer();
        public static readonly Keeper Keeper = new Keeper();
        public static readonly MartialArtist MartialArtist = new MartialArtist();
        public static readonly MetaPhysicist MetaPhysicist = new MetaPhysicist();
        public static readonly NanoTechnician NanoTechnician = new NanoTechnician();
        public static readonly Shade Shade = new Shade();
        public static readonly Soldier Soldier = new Soldier();
        public static readonly Trader Trader = new Trader();
        public static readonly Unknown Unknown = new Unknown();

        public static readonly IReadOnlyList<Profession> All = new Profession[]
        {
            Adventurer, Agent, Bureaucrat, Doctor, Enforcer, Engineer, Fixer, Keeper,
            MartialArtist, MetaPhysicist, NanoTechnician, Shade, Soldier, Trader, Unknown
        };

        public abstract string Name { get; }
        public abstract Color Color { get; }
        public abstract Bitmap Icon { get; }

        public static Profession SetProfession(string playerName)
        {
            try
            {
                using (var reader =  new XmlTextReader("http://people.anarchy-online.com/character/bio/d/5/name/" + playerName + "/bio.xml"))
                {
                    while (reader.Read())
                    {
                        if (reader.Name == "profession")
                            return Profession.All.FirstOrDefault(p => p.Name == reader.ReadInnerXml()) ?? Profession.Unknown;
                    }
                }
            }
            catch { }

            return Profession.Unknown;
        }
    }
}