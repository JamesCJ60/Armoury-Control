using acControl.Scripts;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Controls;

namespace acControl.Views.Windows
{
    /// <summary>
    /// Interaction logic for XG_Mobile_Prompt.xaml
    /// </summary>
    public partial class XG_Mobile_Prompt : UiWindow
    {
        private volatile bool eGPUConnected = false;
        
        private DispatcherTimer progressBarTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        public XG_Mobile_Prompt()
        {
            InitializeComponent();

            eGPUConnected = IsEGPUConnected();
            
            UpdateImg(App.location + "\\Images\\XGMobile\\XGMobile-1.png");

            if(!eGPUConnected)
            {
                ToggleButton.Content = "Start Activation Process";
                tbxInfo.Text = "Press \"Start Activation Process\" to begin the activation process of your ROG XG Mobile. \n\n\nWARNING: Do not attempt without an ROG XG Mobile!";
            }
            else
            {
                ToggleButton.Content = "Start Deactivation Process";
                tbxInfo.Text = "Press \"Start Deactivation Process\" to begin the deactivation process of your ROG XG Mobile. \n\n\nWARNING: Do not attempt without an ROG XG Mobile!";
            }
        }

        private bool IsEGPUConnected()
        {
            int deviceStatus = App.wmi.DeviceGet(ASUSWmi.eGPU);
            if (deviceStatus != 0 && deviceStatus != 1)
            {
                throw new InvalidOperationException($"Unknown device status: {deviceStatus}");
            }
            return App.wmi.DeviceGet(ASUSWmi.eGPU) == 1;
        }

        private void UpdateImg(string path)
        {
          imgDiagram.Source  = new BitmapImage(new Uri(path));
        }

        private void btn2_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void btn1_Click(object sender, RoutedEventArgs e)
        {
            ToggleXgMobile();
        }
       
        private void UpdateProgress(bool statusToWait)
        {
            if (eGPUConnected != statusToWait)
            {
                pbStatus.Value = pbStatus.Value != 88 ? pbStatus.Value + 1 : 88;
            }
            else
            {
                pbStatus.Value += 5;
                if (pbStatus.Value >= 100)
                {
                    FinishProgress();
                }
            }            
        }

        private void FinishProgress()
        {
            progressBarTimer.Stop();
            if (eGPUConnected)
            {
                tbxInfo.Text = "Your ROG XG Mobile is now activated! \n\n\nWARNING: Do not remove ROG XG Mobile from device until it has been deactivated!";
            }
            else
            {
                tbxInfo.Text = "Your ROG XG Mobile is now deactivated. You can now safely detach it!";
                updateLHM();
            }
            CloseButton.IsEnabled = true;
        }
       
        private void ToggleXgMobile()
        {
            bool statusToSwitchTo = !eGPUConnected;
            ToggleButton.Visibility = Visibility.Collapsed;
            pbStatus.Visibility = Visibility.Visible;
            CloseButton.IsEnabled = false;
            Task.Run(() => {
                SetXgMobileStatus(statusToSwitchTo); // changing status is blocking operation
                eGPUConnected = IsEGPUConnected();
            });
            progressBarTimer.Tick += (_, _) => { UpdateProgress(statusToSwitchTo); };
            progressBarTimer.Start();
        }

        private void SetXgMobileStatus(bool connected)
        {
            App.wmi.DeviceSet(ASUSWmi.eGPU, connected ? 1 : 0);
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
