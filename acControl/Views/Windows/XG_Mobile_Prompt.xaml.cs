using acControl.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace acControl.Views.Windows
{
    /// <summary>
    /// Interaction logic for XG_Mobile_Prompt.xaml
    /// </summary>
    public partial class XG_Mobile_Prompt : UiWindow
    {
        public XG_Mobile_Prompt()
        {
            InitializeComponent();

            try
            {
                eGPU = App.wmi.DeviceGet(ASUSWmi.eGPU);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            UpdateImg(App.location + "\\Images\\XGMobile\\XGMobile-1.png");

            if(eGPU == 0)
            {
                btn1.Content = "Start Activation Process";
                tbxInfo.Text = "Press \"Start Activation Process\" to begin the activation process of your ROG XG Mobile. \n\n\nWARNING: Do not attempt without an ROG XG Mobile!";
            }
            else
            {
                btn1.Content = "Start Deactivation Process";
                tbxInfo.Text = "Press \"Start Deactivation Process\" to begin the deactivation process of your ROG XG Mobile. \n\n\nWARNING: Do not attempt without an ROG XG Mobile!";
            }
        }

        private void UpdateImg(string path)
        {
          imgDiagram.Source  = new BitmapImage(new Uri(path));
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        int eGPU = 1;

        DispatcherTimer eGPUStatus = new DispatcherTimer();
        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            btn2.IsEnabled = false;
            btn1.Visibility = Visibility.Collapsed;
            if (eGPU == 0)
            {
                pbStatus.Visibility = Visibility.Visible;
                tbxInfo.Text = "Please wait patiently while we try to activate your ROG XG Mobile! \n\n\nThe ROG XG Mobile's connector status indicator LED will display red once finished.";
                eGPUStatus.Interval = TimeSpan.FromSeconds(0.09);
                eGPUStatus.Tick += eGPUStatus_Tick;
                eGPUStatus.Start();
                started = 1;

                App.wmi.DeviceSet(ASUSWmi.eGPU, 1);
            }
            else
            {
                pbStatus.Visibility = Visibility.Visible;
                tbxInfo.Text = "Please wait patiently while we try to deactivate your ROG XG Mobile! \n\n\nThe ROG XG Mobile's connector status indicator LED will display white once finished.";
                
                eGPUStatus.Interval = TimeSpan.FromSeconds(0.09);
                eGPUStatus.Tick += eGPUStatus_Tick;
                eGPUStatus.Start();
                started = 0;

                App.wmi.DeviceSet(ASUSWmi.eGPU, 0);
            }
        }
        int i = 0;
        int step = 0;
        int started = 0;
        void eGPUStatus_Tick(object sender, EventArgs e)
        {
            try
            {
                eGPU = App.wmi.DeviceGet(ASUSWmi.eGPU);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            if (started == 1)
            {
                pbStatus.Value = i;
                i++;
                step++;
                if (step < 505)
                {
                   if(i >= 88) i = 88;
                }
                else if (step >= 505)
                {
                    if(eGPU == 1)
                    {
                        if (i >= 100) i = 100;
                        pbStatus.Value = i;
                        btn2.IsEnabled = true;
                        tbxInfo.Text = "Your ROG XG Mobile is now activated! \n\n\nWARNING: Do not remove ROG XG Mobile from device until it has been deactivated!";
                        if (i >= 100) eGPUStatus.Stop();
                    }
                    else
                    {
                        if (i >= 100) i = 100;
                        pbStatus.Value = i;
                        btn2.IsEnabled = true;
                        tbxInfo.Text = "Your ROG XG Mobile activation failed!";
                        eGPUStatus.Stop();
                    }
                }
            }
            else
            {
                pbStatus.Value = i;
                i++;
                step++;
                if (step < 505)
                {
                    if (i >= 88) i = 88;
                }
                else if (step >= 505)
                {
                    if (eGPU == 0)
                    {
                        if (i >= 100) i = 100;
                        pbStatus.Value = i;
                        btn2.IsEnabled = true;
                        tbxInfo.Text = "Your ROG XG Mobile is now deactivated. You can now safly detach it!";
                        if (i >= 100) eGPUStatus.Stop();
                        updateLHM();
                    }
                    else
                    {
                        if (i >= 100) i = 100;
                        pbStatus.Value = i;
                        btn2.IsEnabled = true;
                        tbxInfo.Text = "Your ROG XG Mobile deactivation failed!";
                        eGPUStatus.Stop();
                        updateLHM();
                    }
                }
            }
            
        }

        async void updateLHM()
        {
            await Task.Run(() =>
            {

                Thread.Sleep(1000);

                GetSystemInfo.stop();

                Thread.Sleep(1000);
                GetSystemInfo.start();

                try
                {
                    GarbageCollection.Garbage_Collect();
                }
                catch { }
            });
        }
    }
}
