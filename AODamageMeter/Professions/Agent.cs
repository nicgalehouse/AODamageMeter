using AODamageMeter.Properties;
using System.Drawing;

namespace AODamageMeter.Professions
{
    public class Agent : Profession
    {
        public override string Name => "Agent";
        public override Color Color => Color.FromArgb(156, 39, 176); // #9C27B0
        public override Bitmap Icon => Resources.Agent;
    }
}
