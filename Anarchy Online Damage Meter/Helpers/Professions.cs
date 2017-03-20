using System;
using System.Xml;

namespace Anarchy_Online_Damage_Meter.Helpers
{
    public enum Profession
    {
        Adventurer = 0,
        Agent = 1,
        Bureaucrat = 2,
        Doctor = 3,
        Enforcer = 4,
        Engineer = 5,
        Fixer = 6,
        Keeper = 7,
        MartialArtist = 8,
        MetaPhysicist = 9,
        NanoTechnician = 10,
        Shade = 11,
        Soldier = 12,
        Trader = 13,
        Unknown = 14
    }

    public static class Professions
    {
        static string[] Colors =
            {
            "689F38", //Adventerur
            "9C27B0", //Agent
            "757575", //Bureaucrat
            "C62828", //Doctor
            "5D4037", //Enforcer
            "A1887F", //Engineer
            "1E88E5", //Fixer
            "466387", //Keeper
            "EF6C00", //MartialArtist
            "4527A0", //MetaPhysicist
            "00695C", //NanoTechnician
            "AD1457", //Shade
            "283593", //Soldier
            "827717", //Trader
            "212121"  //Unknown
           };

        public static string GetIcon(Profession profession)
            => "../Icons/" + profession.ToString() + ".png";

        public static string GetProfessionColor(Profession profession)
            => "#" + Colors[(int)profession];

        public static Profession SetProfession(string playerName)
        {
            Profession profession = Profession.Unknown;

            try
            {
                using (XmlTextReader reader = 
                    new XmlTextReader("http://people.anarchy-online.com/character/bio/d/5/name/" + playerName + "/bio.xml"))
                {
                    while (reader.Read())
                    {
                        if (reader.Name == "profession")
                        {
                                                             //"Meta-Physcicist", "Martial Artist"
                            Enum.TryParse(reader.ReadInnerXml().Replace("-", "").Replace(" ", ""),
                                true,
                                out profession);
                        }
                    }
                }
            }
            catch { }

            return profession;
        }
    }
}