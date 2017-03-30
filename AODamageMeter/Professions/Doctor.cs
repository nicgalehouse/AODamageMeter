using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Doctor : Profession
    {
        public override string Name => "Doctor";
        public override Color Color => Color.FromArgb(198, 40, 40); // #C62828
        public override Bitmap Icon => Resources.Doctor;
    }
}
