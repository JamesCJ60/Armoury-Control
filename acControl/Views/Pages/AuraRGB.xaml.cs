using acControl.Properties;
using acControl.Scripts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
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
using Wpf.Ui.Controls;

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

            if (Settings.Default.MinimalGUI == true)
            {
                tbxMode.Visibility = Visibility.Collapsed;
                tbxSpeed.Visibility = Visibility.Collapsed;
            }

            updateGUI();
            updateRGB();
        }

        private void updateRGB()
        {
            System.Drawing.Color color1 = System.Drawing.ColorTranslator.FromHtml(tbC1.Text);
            System.Drawing.Color color2 = System.Drawing.ColorTranslator.FromHtml(tbC2.Text);
            Aura.Color1 = color1;
            Aura.Color2 = color2;

            bColor1.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color1.A, color1.R, color1.G, color1.B));
            bColor2.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color2.A, color2.R, color2.G, color2.B));

            if(cbxSpeed.SelectedIndex == 0) Aura.Speed = 0xe1;
            if (cbxSpeed.SelectedIndex == 1) Aura.Speed = 0xeb;
            if (cbxSpeed.SelectedIndex == 2) Aura.Speed = 0xf5;

            Aura.Mode = cbxMode.SelectedIndex;
        }

        void updateGUI()
        {
            this.Dispatcher.Invoke(() =>
            {
                tbC1.Text = Settings.Default.HexC1Profile1;
                tbC2.Text = Settings.Default.HexC1Profile2;
                updateRGB();
            });
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if(IsHexColor(tbC1.Text) && IsHexColor(tbC2.Text))
            {
                Settings.Default.HexC1Profile1 = tbC1.Text;
                Settings.Default.HexC2Profile1 = tbC2.Text;
                Settings.Default.AuraMode1 = cbxMode.SelectedIndex;
                Settings.Default.AuraSpeed = cbxSpeed.SelectedIndex;
                Settings.Default.Save();
            }
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            updateGUI();
        }

        private bool IsHexColor(string input)
        {
            if (input.StartsWith("#") && input.Length == 7 || input.Length == 9)
            {
                string hex = input.Replace("#", "");

                if (hex.Length == 6 || hex.Length == 8)
                {
                    foreach (char c in hex)
                    {
                        if (!char.IsDigit(c) && !("ABCDEFabcdef").Contains(c))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (IsHexColor(tbC1.Text) && IsHexColor(tbC2.Text))
            {
                Settings.Default.HexC1Profile1 = tbC1.Text;
                Settings.Default.HexC2Profile1 = tbC2.Text;
                Settings.Default.AuraMode1 = cbxMode.SelectedIndex;
                Settings.Default.AuraSpeed = cbxSpeed.SelectedIndex;
                Settings.Default.Save();

                updateGUI();
                updateRGB();
                Aura.ApplyAura();
            }
        }

        private void tbC1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsHexColor(tbC1.Text))
            {
                tbC1.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            else
            {
                tbC1.Foreground = new SolidColorBrush(System.Windows.Media.Colors.White);

                System.Drawing.Color color1 = System.Drawing.ColorTranslator.FromHtml(tbC1.Text);

                Aura.Color1 = color1;

                bColor1.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color1.A, color1.R, color1.G, color1.B));
            }
        }

        private void tbC2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsHexColor(tbC2.Text))
            {
                tbC2.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            else
            {
                tbC2.Foreground = new SolidColorBrush(System.Windows.Media.Colors.White);

                System.Drawing.Color color2 = System.Drawing.ColorTranslator.FromHtml(tbC2.Text);
                Aura.Color2 = color2;

                bColor2.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color2.A, color2.R, color2.G, color2.B));
            }
        }
    }
}
