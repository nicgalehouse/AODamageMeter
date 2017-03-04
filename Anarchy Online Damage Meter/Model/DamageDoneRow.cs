using Anarchy_Online_Damage_Meter.Helpers;

namespace Anarchy_Online_Damage_Meter.Model
{
    public class DamageDoneRow : Row
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        private double _percentOfDamageDone;
        public double PercentOfDamageDone
        {
            get { return _percentOfDamageDone; }
            set { SetProperty(ref _percentOfDamageDone, value); }
        }

        private double _width;
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private double _dps;
        public double DPS
        {
            get { return _dps; }
            set { SetProperty(ref _dps, value); }
        }

        private long _damageDone;
        public long DamageDone
        {
            get { return _damageDone; }
            set { SetProperty(ref _damageDone, value); }
        }

        public void Update(Character character)
        {
            DPS = character.DPSrelativeToPlayerStart;
            DamageDone = character.DamageDone;
            PercentOfDamageDone = character.PercentOfDamageDone;
            Width = character.PercentOfMaxDamage;
        }

        public static DamageDoneRow Create(Character character)
            => new DamageDoneRow
            {
                Name = character.Name,
                Icon = character.Profession != null
                    ? "../Icons/" + character.Profession + ".png"
                    : "../Icons/Unknown2.png",
                DPS = character.DPSrelativeToPlayerStart,
                DamageDone = character.DamageDone,
                PercentOfDamageDone = character.PercentOfDamageDone,
                Width = character.PercentOfMaxDamage,
                Color = "#BF350C"
            };
    }
}
