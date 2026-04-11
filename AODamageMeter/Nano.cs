namespace AODamageMeter
{
    public class Nano
    {
        public Nano(string name, string iconPath, string color,
            string shortName = null, double? durationSeconds = null,
            int? tauntAmount = null, int? detauntAmount = null)
        {
            Name = name;
            ShortName = shortName ?? name;
            IconPath = iconPath;
            Color = color;
            DurationSeconds = durationSeconds;
            TauntAmount = tauntAmount;
            DetauntAmount = detauntAmount;
        }

        public string Name { get; }
        public string ShortName { get; }
        public string IconPath { get; }
        public string Color { get; }
        public double? DurationSeconds { get; }
        public int? TauntAmount { get; }
        public int? DetauntAmount { get; }
    }
}
