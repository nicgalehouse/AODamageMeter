using System;

namespace AODamageMeter
{
    public enum Faction
    {
        Unknown,
        Clan,
        Neutral,
        Omni
    }

    public static class FactionHelpers
    {
        public static Faction GetFaction(string value)
        {
            switch (value)
            {
                case "Clan": return Faction.Clan;
                case "Neutral": return Faction.Neutral;
                case "Omni": return Faction.Omni;
                default: throw new ArgumentException(value);
            }
        }
    }
}
