using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
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
            ObservableCollection<Character> listViewCharacters = new ObservableCollection<Character>(anarchyOnline.CurrentFight.CharactersList);
            Thread backEnd = new Thread(MainProgram);
            anarchyOnline.readFile();



           // anarchyOnline = new DamageMeter(@"C:\Users\nicga\AppData\Local\Funcom\Anarchy Online\70dad3e6\Anarchy Online\Prefs\eugenedavid\Char484883214\Chat\Windows\Window4\Log.txt");


            //DamageMeter anarchyOnline = new DamageMeter(@"C:\Users\nicga\AppData\Local\Funcom\Anarchy Online\70dad3e6\Anarchy Online\Prefs\pearljam\Char1088088\Chat\Windows\Window2\Log.txt");
            //DamageMeter anarchyOnline = new DamageMeter(@"C:\Users\nicga\AppData\Local\Funcom\Anarchy Online\70dad3e6\Anarchy Online\Prefs\eugenedavid\Char484883214\Chat\Windows\Window4\Log.txt");

            //ObservableCollection<Character> listViewCharacters = new ObservableCollection<Character>(anarchyOnline.CurrentFight.CharactersList);
           // while (true)
           // {
           //     listViewDamageMeter.ItemsSource = listViewCharacters;
           // }
            ////////////////////////listViewDamageMeter.ItemsSource = characters;

            //listViewDamageMeter.ItemsSource = anarchyOnline.
            //Console.WriteLine("Done Reading");
            //anarchyOnline.listOverallCharacters();
        }
        
        public void MainProgram()
        {
            anarchyOnline.readFile();
            //filename = @"C:\Users\nicga\AppData\Local\Funcom\Anarchy Online\70dad3e6\Anarchy Online\Prefs\eugenedavid\Char484883214\Chat\Windows\Window4\Log.txt";
            //LogFileReader Test = new LogFileReader();
            //System.Threading.Thread.Sleep(5000);
        }
        
    }
}
