namespace AODamageMeter
{
    public class Buff
    {
        public Buff(string name, double durationSeconds, string iconPath, string color, string shortName = null)
        {
            Name = name;
            ShortName = shortName ?? name;
            DurationSeconds = durationSeconds;
            IconPath = iconPath;
            Color = color;
        }

        public string Name { get; }
        public string ShortName { get; }
        public double DurationSeconds { get; }
        public string IconPath { get; }
        public string Color { get; }
    }
}
