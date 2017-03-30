using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Keeper : Profession
    {
        public override string Name => "Keeper";
        public override Color Color => Color.FromArgb(70, 99, 135); // #466387
        public override Bitmap Icon => Resources.Keeper;
    }
}
