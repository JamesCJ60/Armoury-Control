using Wpf.Ui.Common.Interfaces;
using acControl.Scripts;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Threading;
using acControl.ViewModels;
using System.Diagnostics;
using acControl.Properties;
using System.Windows;
using acControl.Views.Windows;
using System.DirectoryServices.ActiveDirectory;
using System.Threading;
using System.Drawing;
using System.Windows.Input;

namespace acControl.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>
    {
        public ViewModels.DashboardViewModel ViewModel
        {
            get;
        }

        public static bool updateProfile = false;

        public DashboardPage(ViewModels.DashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            _ = Tablet.TabletDevices;
            InitializeComponent();
            _ = Tablet.TabletDevices;
            setupGUI();
        }
        private bool setup = false;
        private void setupGUI()
        {
            string deviceName = MotherboardInfo.Product;
            deviceName = deviceName.Substring(0, deviceName.LastIndexOf('_'));
            tbxDeviceName.Text = deviceName;

            tbxCPUName.Text = GetSystemInfo.GetCPUName().Replace("with Radeon Graphics", null);
            tbxiGPUName.Text = GetSystemInfo.GetiGPUName().Replace("(R)", null);
            string dGPU = GetSystemInfo.GetdGPUName().Replace(" GPU", null);
            if (dGPU == null || dGPU == "") spdGPU.Visibility = System.Windows.Visibility.Collapsed;
            else tbxdGPUName.Text = dGPU;

            sdBattery.Value = (int)Settings.Default.BatLimit;
            sdBright.Value = (int)GetSystemInfo.getBrightness();

            //var messageBox = new Wpf.Ui.Controls.MessageBox();

            //messageBox.ButtonLeftName = "Hello World";
            //messageBox.ButtonRightName = "Just close me";

            //messageBox.ButtonLeftClick += MessageBox_LeftButtonClick;
            //messageBox.ButtonRightClick += MessageBox_RightButtonClick;

            //messageBox.Show("Something weird", "May happen");

            lblMinDisplay.Content = $" {GetSystemInfo.minRefreshRate}Hz";
            lblMaxDisplay.Content = $" {GetSystemInfo.maxRefreshRate}Hz";
            lblDisplayAuto.Content = " Auto";
            lblDisplayOver.Content = " OD  ";


            GetSystemInfo.CurrentDisplayRrefresh();

            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(1.75);
            sensor.Tick += SensorUpdate_Tick;
            sensor.Start();

            Global.wasUsingOD = Settings.Default.DisplayOver;
            if (Global.wasUsingOD == true)
            {
                tbDisplayOver.IsChecked = true;
            }

            int overdrive = 0;
            try
            {
                overdrive = App.wmi.DeviceGet(ASUSWmi.ScreenOverdrive);

                if (Global.wasUsingOD == true && overdrive == 0)
                {
                    App.wmi.DeviceSet(ASUSWmi.ScreenOverdrive, 1);
                }
            }
            catch
            {
                Debug.WriteLine("Screen Overdrive not supported");
                Global.wasUsingOD = false;
                Settings.Default.DisplayOver = false;
                Settings.Default.Save();
                tbDisplayOver.Visibility = Visibility.Collapsed;
            }

            switchProfile(Settings.Default.ACMode);

            if (Settings.Default.DisplayMode == 0)
            {
                tbMax.IsChecked = true;
            }
            if (Settings.Default.DisplayMode == 1)
            {
                tbMin.IsChecked = true;
            }
            if (Settings.Default.DisplayMode == 2)
            {
                tbDisplayAuto.IsChecked = true;
                Global.toggleDisplay = true;
            }

            if (Settings.Default.GPUMode == 0)
            {
                tbUlti.IsChecked = true;
            }
            if (Settings.Default.GPUMode == 1)
            {
                tbStan.IsChecked = true;
            }
            if (Settings.Default.GPUMode == 2)
            {
                tbEco.IsChecked = true;
            }
            if (Settings.Default.GPUMode == 3)
            {
                tbAuto.IsChecked = true;
                Global.toggleGPU = true;
            }

            update();

            setup = true;
        }

        async void SensorUpdate_Tick(object sender, EventArgs e)
        {
            update();
        }


        private async void update()
        {
            
            GetSystemInfo.getBattery();

            if(Global.isMinimised == false)
            {
                var cpuFan = App.wmi.DeviceGet(ASUSWmi.CPU_Fan);
                var gpuFan = App.wmi.DeviceGet(ASUSWmi.GPU_Fan);

                tbxCPUFan.Text = $"{cpuFan * 0x64} RPM";
                tbxdGPUFan.Text = $"{gpuFan * 0x64} RPM";
                prdGPUFan.Progress = Math.Round(gpuFan / 0.69);
                prCPUFan.Progress = Math.Round(cpuFan / 0.69);

                tbxCPUPer.Text = $"{Math.Round(cpuFan / 0.69)}%";
                tbxdGPUPer.Text = $"{Math.Round(gpuFan / 0.69)}%";
            }
            

            if (tbAuto.IsChecked == true && setup == true || tbDisplayAuto.IsChecked == true && setup == true)
            {
                if (Global.isMinimised == false)
                {
                    string dGPU = await Task.Run(() => GetSystemInfo.GetdGPUName().Replace(" GPU", null));
                    if (dGPU == null || dGPU == "") spdGPU.Visibility = System.Windows.Visibility.Collapsed;
                    else
                    {
                        spdGPU.Visibility = System.Windows.Visibility.Visible;
                        tbxdGPUName.Text = dGPU;
                    }
                }
            }


            if (updateProfile == true) { switchProfile(Settings.Default.ACMode); updateProfile = false; }
        }
        public async void switchProfile(int ACProfile)
        {
            Settings.Default.ACMode = ACProfile;
            Settings.Default.Save();

            if (Settings.Default.ACMode == 0)
            {
                tbSilent.IsChecked = true;
                tbTurbo.IsChecked = false;
                tbPerf.IsChecked = false;
                tbMan.IsChecked = false;
                imgPerformProfile.Source = new BitmapImage(new Uri(App.location + "\\Images\\ACProfiles\\Silent.png"));
                await Task.Run(() =>
                {
                    App.wmi.DeviceSet(ASUSWmi.PerformanceMode, ASUSWmi.PerformanceSilent);
                });
            }
            if (Settings.Default.ACMode == 1)
            {
                tbPerf.IsChecked = true;
                tbSilent.IsChecked = false;
                tbTurbo.IsChecked = false;
                tbMan.IsChecked = false;
                imgPerformProfile.Source = new BitmapImage(new Uri(App.location + "\\Images\\ACProfiles\\Bal.png"));
                await Task.Run(() =>
                {
                    App.wmi.DeviceSet(ASUSWmi.PerformanceMode, ASUSWmi.PerformanceBalanced);
                });
            }
            if (Settings.Default.ACMode == 2)
            {
                tbTurbo.IsChecked = true;
                tbSilent.IsChecked = false;
                tbPerf.IsChecked = false;
                tbMan.IsChecked = false;
                imgPerformProfile.Source = new BitmapImage(new Uri(App.location + "\\Images\\ACProfiles\\Turbo.png"));
                await Task.Run(() =>
                {
                    App.wmi.DeviceSet(ASUSWmi.PerformanceMode, ASUSWmi.PerformanceTurbo);
                });
            }
            if (Settings.Default.ACMode == 3)
            {
                imgPerformProfile.Source = new BitmapImage(new Uri(App.location + "\\Images\\ACProfiles\\Windows.png"));
            }
        }

        private void sdBright_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (setup) SetSystemSettings.setDisplayBrightness((int)sdBright.Value);
        }

        private void sdBattery_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (setup) SetSystemSettings.setBatteryLimit((int)sdBattery.Value);
        }

        private async void tbMax_Click(object sender, RoutedEventArgs e)
        {
            if (tbMax.IsChecked == false && tbMin.IsChecked == false && tbDisplayAuto.IsChecked == false) tbMax.IsChecked = true;
            tbMin.IsChecked = false;
            tbDisplayAuto.IsChecked = false;
            Global.toggleDisplay = false;
            if (Global.wasUsingOD == true) tbDisplayOver.IsChecked = true;
            SetSystemSettings.setDisplaySettings(1);
            Settings.Default.DisplayMode = 0;
            Settings.Default.Save();
        }

        private async void tbMin_Click(object sender, RoutedEventArgs e)
        {
            if (tbMin.IsChecked == false && tbMax.IsChecked == false && tbDisplayAuto.IsChecked == false) tbMin.IsChecked = true;
            tbMax.IsChecked = false;
            tbDisplayOver.IsChecked = false;
            tbDisplayAuto.IsChecked = false;
            Global.toggleDisplay = false;
            SetSystemSettings.setDisplaySettings(0);
            RunCLI.RunCommand("Powershell.exe (Get-WmiObject -Namespace root/WMI -Class AsusAtkWmi_WMNB).DEVS(0x00050019, 0)", true);
            Settings.Default.DisplayMode = 1;
            Settings.Default.Save();
        }

        private async void tbStan_Click(object sender, RoutedEventArgs e)
        {
            if (tbUlti.IsChecked == false && tbStan.IsChecked == false && tbEco.IsChecked == false && tbAuto.IsChecked == false) tbStan.IsChecked = true;
            tbEco.IsChecked = false;
            tbAuto.IsChecked = false;
            tbUlti.IsChecked = false;
            SetSystemSettings.setGPUSettings(0);
            Settings.Default.GPUMode = 1;
            Settings.Default.Save();
        }

        private async void tbEco_Click(object sender, RoutedEventArgs e)
        {
            if (tbUlti.IsChecked == false && tbStan.IsChecked == false && tbEco.IsChecked == false && tbAuto.IsChecked == false) tbEco.IsChecked = true;
            tbStan.IsChecked = false;
            tbAuto.IsChecked = false;
            tbUlti.IsChecked = false;
            SetSystemSettings.setGPUSettings(1);
            Settings.Default.GPUMode = 2;
            Settings.Default.Save();
        }

        private void tbDisplayOver_Click(object sender, RoutedEventArgs e)
        {
            GetSystemInfo.CurrentDisplayRrefresh();
            if (tbDisplayOver.IsChecked == true && GetSystemInfo.currentRefreshRate == GetSystemInfo.maxRefreshRate)
            {
                Global.wasUsingOD = true;
                SetSystemSettings.setDisplaySettings(1);
            }
            else
            {
                SetSystemSettings.setDisplaySettings(0);
            }
            Settings.Default.Save();
        }

        private void tbTurbo_Click(object sender, RoutedEventArgs e)
        {
            switchProfile(2);
        }

        private void tbPerf_Click(object sender, RoutedEventArgs e)
        {
            switchProfile(1);
        }

        private void tbSilent_Click(object sender, RoutedEventArgs e)
        {
            switchProfile(0);
        }

        private void tbDisplayAuto_Click(object sender, RoutedEventArgs e)
        {
            if (tbMin.IsChecked == false && tbMax.IsChecked == false && tbDisplayAuto.IsChecked == false) tbDisplayAuto.IsChecked = true;
            tbMin.IsChecked = false;
            tbMax.IsChecked = false;
            Global.toggleDisplay = true;

            Settings.Default.DisplayMode = 2;
            Settings.Default.Save();
        }

        private void tbAuto_Click(object sender, RoutedEventArgs e)
        {
            if (tbUlti.IsChecked == false && tbStan.IsChecked == false && tbEco.IsChecked == false && tbAuto.IsChecked == false) tbAuto.IsChecked = true;
            tbUlti.IsChecked = false;
            tbStan.IsChecked = false;
            tbEco.IsChecked = false;
            Global.toggleGPU = true;
            Settings.Default.GPUMode = 3;
            Settings.Default.Save();
        }
    }
}