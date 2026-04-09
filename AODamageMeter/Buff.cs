namespace AODamageMeter
{
    public class Buff
    {
        public Buff(string name, string shortName, double durationSeconds, string iconPath, string color)
        {
            Name = name;
            ShortName = shortName;
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
