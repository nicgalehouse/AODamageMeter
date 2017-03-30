using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Adventurer : Profession
    {
        public override string Name => "Adventurer";
        public override Color Color => Color.FromArgb(104, 159, 56); // #689F38
        public override Bitmap Icon => Resources.Adventurer;
    }
}
