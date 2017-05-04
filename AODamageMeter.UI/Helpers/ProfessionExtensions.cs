using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace AODamageMeter.UI.Helpers
{
    public static class ProfessionExtensions
    {
        private static readonly Dictionary<Profession, string> _professionIconPaths = Profession.All
            .ToDictionary(p => p, p => $"/Icons/{p.GetType().Name}.png");

        private static readonly Dictionary<Profession, Color> _professionColors = Profession.All
            .ToDictionary(p => p, p => Color.FromRgb(p.Color.R, p.Color.G, p.Color.B));

        public static string GetIconPath(this Profession profession)
            => profession == null ? $"/Icons/NPC.png" : _professionIconPaths[profession] ;

        public static Color GetColor(this Profession profession)
            => _professionColors[profession ?? Profession.Unknown];
    }
}
