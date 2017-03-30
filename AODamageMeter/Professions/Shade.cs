using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Shade : Profession
    {
        public override string Name => "Shade";
        public override Color Color => Color.FromArgb(173, 20, 87); // #AD1457
        public override Bitmap Icon => Resources.Shade;
    }
}
