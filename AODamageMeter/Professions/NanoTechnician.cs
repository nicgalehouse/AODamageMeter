using System.Drawing;

namespace AODamageMeter.Professions
{
    public class NanoTechnician : Profession
    {
        protected internal NanoTechnician() { }

        public override string Name => "Nano-Technician";
        public override Color Color => Color.FromArgb(66, 155, 132);
    }
}
