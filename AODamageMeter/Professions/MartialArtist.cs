using System.Drawing;

namespace AODamageMeter.Professions
{
    public class MartialArtist : Profession
    {
        protected internal MartialArtist() { }

        public override string Name => "Martial Artist";
        public override Color Color => Color.FromArgb(183, 120, 62);
    }
}
