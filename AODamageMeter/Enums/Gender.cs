using System;

namespace AODamageMeter
{
    public enum Gender
    {
        Unknown,
        Female,
        Male,
        Uni
    }

    public static class GenderHelpers
    {
        public static Gender GetGender(string value)
        {
            switch (value)
            {
                case "Female": return Gender.Female;
                case "Male": return Gender.Male;
                case "Neuter": return Gender.Uni;
                case "Uni": return Gender.Uni;
                default: throw new ArgumentException(value);
            }
        }
    }
}
