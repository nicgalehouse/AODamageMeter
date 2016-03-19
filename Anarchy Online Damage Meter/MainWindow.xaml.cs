using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Anarchy_Online_Damage_Meter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //MainProgram();
        public MainWindow()
        {

                InitializeComponent();

                DamageMeter anarchyOnline = new DamageMeter(@"C:\Users\nicga\AppData\Local\Funcom\Anarchy Online\70dad3e6\Anarchy Online\Prefs\eugenedavid\Char484883214\Chat\Windows\Window4\Log.txt");

        }

        public void MainProgram()
        {
            //filename = @"C:\Users\nicga\AppData\Local\Funcom\Anarchy Online\70dad3e6\Anarchy Online\Prefs\eugenedavid\Char484883214\Chat\Windows\Window4\Log.txt";
            //LogFileReader Test = new LogFileReader();
            //System.Threading.Thread.Sleep(5000);
        }
    }
}
