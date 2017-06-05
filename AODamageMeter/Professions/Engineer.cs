using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Engineer : Profession
    {
        protected internal Engineer() { }

        public override string Name => "Engineer";
        public override Color Color => Color.FromArgb(118, 126, 135);
    }
}
