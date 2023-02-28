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
        public static int mux = App.wmi.DeviceGet(ASUSWmi.GPUMux);
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
            mux = App.wmi.DeviceGet(ASUSWmi.GPUMux);
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
            SetSystemSettings.setBatteryLimit((int)sdBattery.Value);

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

        int eGPU = 1;
        private async void update()
        {
            if (Global.isMinimised == false)
            {
                if (tbxDeviceName.Text.Contains("Flow"))
                {
                    if (cdXGMobile.Visibility == Visibility.Collapsed) { cdXGMobile.Visibility = Visibility.Visible; }

                    try
                    {
                        eGPU = App.wmi.DeviceGet(ASUSWmi.eGPU);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }

                    if (eGPU == 0)
                    { lblXGMobile.Content = "Activate ROG XG Mobile"; cdGPU.Visibility = Visibility.Visible; }
                    else { lblXGMobile.Content = "Deactivate ROG XG Mobile"; cdGPU.Visibility = Visibility.Collapsed; }

                    if(mux < 1 && tbXG.IsEnabled == true) { tbXG.IsEnabled = false; }
                    else { tbXG.IsEnabled = true; }
                }

                var cpuFan = App.wmi.DeviceGet(ASUSWmi.CPU_Fan);
                var gpuFan = App.wmi.DeviceGet(ASUSWmi.GPU_Fan);


                tbxCPUFan.Text = $"{cpuFan * 0x64} RPM";
                tbxdGPUFan.Text = $"{gpuFan * 0x64} RPM";
                prdGPUFan.Progress = Math.Round(gpuFan / 0.69);
                prCPUFan.Progress = Math.Round(cpuFan / 0.69);

                tbxCPUPer.Text = $"{Math.Round(cpuFan / 0.69)}%";
                tbxdGPUPer.Text = $"{Math.Round(gpuFan / 0.69)}%";

                if (tbxCPUFan.Text.Contains("-") || tbxdGPUFan.Text.Contains("-"))
                {
                    cpuFan = App.wmi.DeviceGet2(ASUSWmi.CPU_Fan);
                    gpuFan = App.wmi.DeviceGet2(ASUSWmi.GPU_Fan);

                    tbxCPUFan.Text = $"{cpuFan * 0x64} RPM";
                    tbxdGPUFan.Text = $"{gpuFan * 0x64} RPM";
                    prdGPUFan.Progress = Math.Round(gpuFan / 0.69);
                    prCPUFan.Progress = Math.Round(cpuFan / 0.69);

                    tbxCPUPer.Text = $"{Math.Round(cpuFan / 0.69)}%";
                    tbxdGPUPer.Text = $"{Math.Round(gpuFan / 0.69)}%";
                }
                
            }

            if (tbAuto.IsChecked == true && setup == true || tbDisplayAuto.IsChecked == true && setup == true)
            {
                SetSystemSettings.setACDCSettings();

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
                tbTurbo.IsChecked = false;
                tbSilent.IsChecked = false;
                tbPerf.IsChecked = false;
                tbMan.IsChecked = true;
                imgPerformProfile.Source = new BitmapImage(new Uri(App.location + "\\Images\\ACProfiles\\Windows.png"));
                App.wmi.DeviceSet(ASUSWmi.PerformanceMode, ASUSWmi.PerformanceBalanced);
                SetSystemSettings.ApplyPresetSettings();
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
            tbMin.IsChecked = true;
            tbMax.IsChecked = false;
            tbDisplayOver.IsChecked = false;
            tbDisplayAuto.IsChecked = false;
            Global.toggleDisplay = false;
            SetSystemSettings.setDisplaySettings(0);
            Settings.Default.DisplayMode = 1;
            Settings.Default.Save();
        }

        private async void tbStan_Click(object sender, RoutedEventArgs e)
        {
            tbStan.IsChecked = false;
            if (mux > 0)
            {
                tbStan.IsChecked = true;
                tbEco.IsChecked = false;
                tbAuto.IsChecked = false;
                tbUlti.IsChecked = false;
                SetSystemSettings.setGPUSettings(0);
                Settings.Default.GPUMode = 1;
                Settings.Default.Save();
            }
            else
            {
                changeMode = 1;
                disbaleUltiMode();
            }
        }

        private async void tbEco_Click(object sender, RoutedEventArgs e)
        {
            tbEco.IsChecked = false;
            if (mux > 0)
            {
                tbEco.IsChecked = true;
                tbStan.IsChecked = false;
                tbAuto.IsChecked = false;
                tbUlti.IsChecked = false;
                SetSystemSettings.setGPUSettings(1);
                Settings.Default.GPUMode = 2;
                Settings.Default.Save();
            }
            else
            {
                changeMode = 2;
                disbaleUltiMode();
            }
        }

        private void tbDisplayOver_Click(object sender, RoutedEventArgs e)
        {
            GetSystemInfo.CurrentDisplayRrefresh();
            if (tbDisplayOver.IsChecked == true)
            {
                Global.wasUsingOD = true;
                SetSystemSettings.setDisplayOver(1);
            }
            else
            {
                Global.wasUsingOD = false;
                SetSystemSettings.setDisplayOver(0);
            }
            Settings.Default.Save();
        }
        private void tbMan_Click(object sender, RoutedEventArgs e)
        {
            switchProfile(3);
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
            tbAuto.IsChecked = false;
            
            if (mux > 0)
            {
                if (tbUlti.IsChecked == false && tbStan.IsChecked == false && tbEco.IsChecked == false && tbAuto.IsChecked == false) tbAuto.IsChecked = true;
                tbUlti.IsChecked = false;
                tbStan.IsChecked = false;
                tbEco.IsChecked = false;
                Global.toggleGPU = true;
                Settings.Default.GPUMode = 3;
                Settings.Default.Save();
            }
            else
            {
                changeMode = 3;
                disbaleUltiMode();
            }
        }

        private void tbUlti_Click(object sender, RoutedEventArgs e)
        {
            int mux = App.wmi.DeviceGet(ASUSWmi.GPUMux);
            tbUlti.IsChecked = false;
            if (mux > 0)
            {

                var messageBox = new Wpf.Ui.Controls.MessageBox();

                messageBox.ButtonLeftName = "Restart";
                messageBox.ButtonRightName = "Cancel";

                messageBox.ButtonLeftClick += MessageBox_Enable;
                messageBox.ButtonRightClick += MessageBox_Close;

                messageBox.Show("GPU Ultimate Mode", "Switching the GPU to Ultimate Mode requires a restart to take\naffect!");
            }
            else if (mux == 0)
            {
                if (tbUlti.IsChecked == false && tbStan.IsChecked == false && tbEco.IsChecked == false && tbAuto.IsChecked == false) tbUlti.IsChecked = true;
                tbAuto.IsChecked = false;
                tbStan.IsChecked = false;
                tbEco.IsChecked = false;
                Settings.Default.GPUMode = 0;
                Settings.Default.Save();
            }
        }

        int changeMode = 1;
        private void disbaleUltiMode()
        {
            int mux = App.wmi.DeviceGet(ASUSWmi.GPUMux);
            tbUlti.IsChecked = false;
            if (mux < 1)
            {
                var messageBox = new Wpf.Ui.Controls.MessageBox();

                messageBox.ButtonLeftName = "Restart";
                messageBox.ButtonRightName = "Cancel";

                messageBox.ButtonLeftClick += MessageBox_Disable;
                messageBox.ButtonRightClick += MessageBox_Close;

                messageBox.Show("GPU Ultimate Mode", "Disbaling the GPU Ultimate Mode requires a restart to take\naffect!");
            }
        }

        private void MessageBox_Enable(object sender, System.Windows.RoutedEventArgs e)
        {
            if (tbUlti.IsChecked == false && tbStan.IsChecked == false && tbEco.IsChecked == false && tbAuto.IsChecked == false) tbUlti.IsChecked = true;
            tbAuto.IsChecked = false;
            tbStan.IsChecked = false;
            tbEco.IsChecked = false;
            Settings.Default.GPUMode = 0;
            Settings.Default.Save();
            App.wmi.DeviceSet(ASUSWmi.GPUMux, 0);
            Thread.Sleep(250);
            Process.Start("shutdown", "/r /t 1");

            (sender as Wpf.Ui.Controls.MessageBox)?.Close();
        }

        private void MessageBox_Disable(object sender, System.Windows.RoutedEventArgs e)
        {
            tbAuto.IsChecked = false;
            tbStan.IsChecked = false;
            tbEco.IsChecked = false;
            tbUlti.IsChecked = false;

            if(changeMode == 1) tbStan.IsChecked = true;
            if (changeMode == 2) tbEco.IsChecked = true;
            if (changeMode == 3) tbAuto.IsChecked = true;

            Settings.Default.GPUMode = changeMode;
            Settings.Default.Save();

            App.wmi.DeviceSet(ASUSWmi.GPUMux, 1);
            Thread.Sleep(250);
            Process.Start("shutdown", "/r /t 1");

            (sender as Wpf.Ui.Controls.MessageBox)?.Close();
        }

        private void MessageBox_Close(object sender, System.Windows.RoutedEventArgs e)
        {
            (sender as Wpf.Ui.Controls.MessageBox)?.Close();
        }

        private void tbXG_Click(object sender, RoutedEventArgs e)
        {
            tbXG.IsChecked = false;
            if (mux > 0)
            {
                if(eGPU < 1)
                {
                    tbStan.IsChecked = false;
                    tbStan.IsChecked = true;
                    tbEco.IsChecked = false;
                    tbAuto.IsChecked = false;
                    tbUlti.IsChecked = false;
                    SetSystemSettings.setGPUSettings(0);
                    Settings.Default.GPUMode = 1;
                    Settings.Default.Save();
                }

                XG_Mobile_Prompt xg = new XG_Mobile_Prompt();
                xg.Show();
            }
        }
    }
}