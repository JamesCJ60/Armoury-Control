using acControl.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace acControl.Views.Pages
{
    /// <summary>
    /// Interaction logic for AuraRGB.xaml
    /// </summary>
    public partial class AuraRGB : Page
    {
        public AuraRGB()
        {
            InitializeComponent();

            if(Settings.Default.MinimalGUI == true)
            {
                tbxMode.Visibility = Visibility.Collapsed;
                tbxSpeed.Visibility = Visibility.Collapsed;
            }
        }

        private void RGBUpdate(object sender, TextChangedEventArgs e)
        {
            updateRGB();
        }

        private void updateRGB()
        {
            Aura.Color1 = System.Drawing.Color.FromArgb((int)nudC1Red.Value, (int)nudC1Green.Value, (int)nudC1Blue.Value);
            Aura.Color2 = System.Drawing.Color.FromArgb((int)nudC2Red.Value, (int)nudC2Green.Value, (int)nudC2Blue.Value);

            bColor1.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)nudC1Red.Value, (byte)nudC1Green.Value, (byte)nudC1Blue.Value));
            bColor2.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)nudC2Red.Value, (byte)nudC2Green.Value, (byte)nudC2Blue.Value));
        }
    }
}
