using System;

namespace AODamageMeter
{
    public enum Breed
    {
        Unknown,
        Atrox,
        Nanomage,
        Opifex,
        Solitus
    }

    public static class BreedHelpers
    {
        public static Breed GetBreed(string value)
        {
            switch (value)
            {
                case "Atrox": return Breed.Atrox;;
                case "Nanomage": return Breed.Nanomage;
                case "Nano": return Breed.Nanomage;
                case "Opifex": return Breed.Opifex;
                case "Solitus": return Breed.Solitus;
                default: throw new ArgumentException(value);
            }
        }
    }
}
