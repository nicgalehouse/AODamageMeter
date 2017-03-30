using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Unknown : Profession
    {
        public override string Name => "Unknown";
        public override Color Color => Color.FromArgb(33, 33, 33); // #212121
        public override Bitmap Icon => Resources.Unknown;
    }
}
