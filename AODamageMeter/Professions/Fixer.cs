using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Fixer : Profession
    {
        protected internal Fixer() { }

        public override string Name => "Fixer";
        public override Color Color => Color.FromArgb(43, 105, 220);
    }
}
