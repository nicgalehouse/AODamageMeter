using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Bureaucrat : Profession
    {
        public override string Name => "Bureaucrat";
        public override Color Color => Color.FromArgb(117, 117, 117); // #757575
        public override Bitmap Icon => Resources.Bureaucrat;
    }
}
