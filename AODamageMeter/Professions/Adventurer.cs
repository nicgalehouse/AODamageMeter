using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Adventurer : Profession
    {
        protected internal Adventurer() { }

        public override string Name => "Adventurer";
        public override Color Color => Color.FromArgb(84, 68, 67);
    }
}
