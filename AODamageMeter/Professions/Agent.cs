using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Agent : Profession
    {
        protected internal Agent() { }

        public override string Name => "Agent";
        public override Color Color => Color.FromArgb(84, 117, 165);
    }
}
