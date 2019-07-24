using System;
using System.Collections.Generic;
using System.Linq;

namespace AODamageMeter
{
    public enum Dimension
    {
        RubiKa,
        RubiKa2019
    }

    public static class DimensionHelpers
    {
        public static Dimension GetDimensionOrDefault(string value, Dimension @default = Dimension.RubiKa)
        {
            switch (value)
            {
                case "Rubi-Ka": return Dimension.RubiKa;
                case "Rubi-Ka 2019": return Dimension.RubiKa2019;
                default: return @default;
            }
        }

        public static string GetName(this Dimension dimension)
        {
            switch (dimension)
            {
                case Dimension.RubiKa: return "Rubi-Ka";
                case Dimension.RubiKa2019: return "Rubi-Ka 2019";
                default: throw new NotImplementedException();
            }
        }

        public static int GetDimensionID(this Dimension dimension)
        {
            switch (dimension)
            {
                case Dimension.RubiKa: return 5;
                case Dimension.RubiKa2019: return 6;
                default: throw new NotImplementedException();
            }
        }

        public static readonly IReadOnlyList<Dimension> AllDimensions = Enum.GetValues(typeof(Dimension))
            .Cast<Dimension>()
            .OrderBy(d => d)
            .ToArray();
    }
}
