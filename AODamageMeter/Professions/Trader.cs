using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Trader : Profession
    {
        public override string Name => "Trader";
        public override Color Color => Color.FromArgb(130, 119, 23); // #827717
        public override Bitmap Icon => Resources.Trader;
    }
}
