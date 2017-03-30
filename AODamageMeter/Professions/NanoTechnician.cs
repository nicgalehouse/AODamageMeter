using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class NanoTechnician : Profession
    {
        public override string Name => "Nano-Technician";
        public override Color Color => Color.FromArgb(0, 105, 92); // #00695C
        public override Bitmap Icon => Resources.NanoTechnician;
    }
}
