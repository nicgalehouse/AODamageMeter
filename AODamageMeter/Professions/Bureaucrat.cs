using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Bureaucrat : Profession
    {
        protected internal Bureaucrat() { }

        public override string Name => "Bureaucrat";
        public override Color Color => Color.FromArgb(85, 85, 130);
    }
}
