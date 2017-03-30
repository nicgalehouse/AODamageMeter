using Prism.Mvvm;
using System;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace AODamageMeter.UI.ViewModels.Rows
{
    public class DamageDoneRowViewModel : RowViewModelBase
    {
        public string Name { get; set; }
        public Bitmap Icon { get; set; }
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

        private string _displayDPS;
        public string DisplayDPS
        {
            get { return _displayDPS; }
            set
            {
                double dps = double.Parse(value);
                string formatted;

                if (dps >= 1000)
                {
                    formatted = Math.Round(dps / 1000, 1).ToString() + "k";
                }
                else
                {
                    formatted = dps.ToString("#,##");
                }

                BitmapImage x;

                SetProperty(ref _displayDPS, formatted);
            }
        }

        private long _damageDone;
        public long DamageDone
        {
            get { return _damageDone; }
            set { SetProperty(ref _damageDone, value); }
        }

        private string _displayDamageDone;
        public string DisplayDamageDone
        {
            get { return _displayDamageDone; }
            set { SetProperty(ref _displayDamageDone,double.Parse(value).ToString("#,##")); }
        }

        public void Update(FightCharacter character)
        {
            DPS = character.DPSrelativeToPlayerStart;
            DamageDone = character.DamageDone;
            PercentOfDamageDone = character.PercentOfDamageDone;

            DisplayDPS = DPS.ToString();
            DisplayDamageDone = DamageDone.ToString();
            Width = character.PercentOfMaxDamage;
        }

        public static DamageDoneRowViewModel Create(FightCharacter character)
            => new DamageDoneRowViewModel
            {
                DPS = character.DPSrelativeToPlayerStart,
                DamageDone = character.DamageDone,
                PercentOfDamageDone = character.PercentOfDamageDone,

                Name = character.Name,
                Icon = Professions.GetIcon(character.Profession),
                DisplayDPS = character.DPSrelativeToPlayerStart.ToString(),
                DisplayDamageDone = character.DamageDone.ToString(),
                Width = character.PercentOfMaxDamage,
                Color = Professions.GetProfessionColor(character.Profession)
            };
    }
}
