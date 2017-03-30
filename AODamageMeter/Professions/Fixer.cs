using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Fixer : Profession
    {
        public override string Name => "Fixer";
        public override Color Color => Color.FromArgb(30, 136, 229); // #1E88E5
        public override Bitmap Icon => Resources.Fixer;
    }
}
