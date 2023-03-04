using acControl.Scripts;
using acControl.Services;
using System;
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
        private readonly XgMobileConnectionService xgMobileConnectionService;

        private DispatcherTimer progressBarTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };

        public XG_Mobile_Prompt(XgMobileConnectionService xgMobileConnectionService) : this(xgMobileConnectionService, false)
        {
        }

        public XG_Mobile_Prompt(XgMobileConnectionService xgMobileConnectionService, bool activate)
        {
            InitializeComponent();
            this.xgMobileConnectionService = xgMobileConnectionService;

            xgMobileConnectionService.XgMobileStatus += OnXgMobileDetected;


            UpdateImg(App.location + "\\Images\\XGMobile\\XGMobile-1.png");

            if (!activate)
            {

                if (!IsEGPUConnected())
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
            else
            {
                ToggleButton.Content = "Start Activation Process";
                tbxInfo.Text = "Press \"Start Activation Process\" to begin the activation process of your ROG XG Mobile. \n\n\nWARNING: Do not attempt without an ROG XG Mobile!";
                ToggleXgMobile();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            xgMobileConnectionService.XgMobileStatus -= OnXgMobileDetected;
        }

        private void OnXgMobileDetected(object? _, XgMobileConnectionService.XgMobileStatusEvent e)
        {
            if (!e.Detected)
            {
                this.Close();
            }
        }

        private bool IsEGPUConnected()
        {
            return xgMobileConnectionService.Connected;
        }

        private void UpdateImg(string path)
        {
            imgDiagram.Source = new BitmapImage(new Uri(path));
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
            if (IsEGPUConnected() != statusToWait)
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
            if (IsEGPUConnected())
            {
                tbxInfo.Text = "Your ROG XG Mobile is now activated! \n\n\nWARNING: Do not remove ROG XG Mobile from device until it has been deactivated!";
                updateLHM();
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
            bool statusToSwitchTo = !IsEGPUConnected();
            ToggleButton.Visibility = Visibility.Collapsed;
            pbStatus.Visibility = Visibility.Visible;
            CloseButton.IsEnabled = false;
            Task.Run(() =>
            {
                SetXgMobileStatus(statusToSwitchTo); // changing status is blocking operation
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

                    if (IsEGPUConnected())
                    {
                        GetSystemInfo.GetdGPUStats();
                        string name = GetSystemInfo.GetdGPUName();
                        name.Replace("GPU", null);

                        if (name.Contains("4090")) ToastNotification.ShowToastNotification(true, "ROG XG Mobile Detected", $"Armoury Control has detected ROG XG Mobile GC33Y ({name}) has been activated!");
                        if (name.Contains("6850M")) ToastNotification.ShowToastNotification(true, "ROG XG Mobile Detected", $"Armoury Control has detected ROG XG Mobile GC32L ({name}) has been activated!");
                        if (name.Contains("3080")) ToastNotification.ShowToastNotification(true, "ROG XG Mobile Detected", $"Armoury Control has detected ROG XG Mobile GC31S ({name}) has been activated!");
                        if (name.Contains("3070")) ToastNotification.ShowToastNotification(true, "ROG XG Mobile Detected", $"Armoury Control has detected ROG XG Mobile GC31R ({name}) has been activated!");
                    }
                }
                catch { }
            });
        }
    }
}
