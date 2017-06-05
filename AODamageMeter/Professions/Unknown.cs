using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Unknown : Profession
    {
        protected internal Unknown() { }

        public override string Name => "Unknown";
        public override Color Color => Color.FromArgb(51, 73, 61);
    }
}
