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
using Microsoft.Win32;
using System.Collections.Generic;
using acControl.Services;
using acControl.Models;

namespace acControl.Views.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : INavigableView<ViewModels.DashboardViewModel>
    {
        public int mux = 0;
        private readonly XgMobileConnectionService xgMobileConnectionService;
        public ViewModels.DashboardViewModel ViewModel
        {
            get;
        }

        public static bool updateProfile = false;

        public DashboardPage(ViewModels.DashboardViewModel viewModel, XgMobileConnectionService xgMobileConnectionService)
        {
            ViewModel = viewModel;
            _ = Tablet.TabletDevices;
            InitializeComponent();
            mux = GetMux();
            this.xgMobileConnectionService = xgMobileConnectionService;
            xgMobileConnectionService.XgMobileStatus += OnXgMobileStatusUpdate;
            this.Unloaded += (_, _) => xgMobileConnectionService.XgMobileStatus -= OnXgMobileStatusUpdate;
            UpdateXgMobileStatus(xgMobileConnectionService.Detected, xgMobileConnectionService.Connected);
            _ = Tablet.TabletDevices;
            setupGUI();
        }
        private bool setup = false;
        private bool hasSysFan = false;
        private async void setupGUI()
        {
            if (Global.isMinimalGUI)
            {
                cdPowerModes.Visibility = Visibility.Visible;
                svMain.Margin = new Thickness(0,6,0,12);
                spDevice.Visibility = Visibility.Visible;
                string deviceName = MotherboardInfo.Product;
                if (deviceName.Contains("_")) deviceName = deviceName.Substring(0, deviceName.LastIndexOf('_'));
                if (!deviceName.Contains("ASUS")) deviceName = "ASUS " + deviceName;
                tbxDeviceName2.Text = deviceName;
                tbxDeviceName.Text = null;
            }
            else
            {
                string deviceName = MotherboardInfo.Product;
                if (deviceName.Contains("_")) deviceName = deviceName.Substring(0, deviceName.LastIndexOf('_'));
                if (!deviceName.Contains("ASUS")) deviceName = "ASUS " + deviceName;
                tbxDeviceName.Text = deviceName;
            }

            
            Global.cpuName = GetSystemInfo.GetCPUName().Replace("with Radeon Graphics", null);
            Global.cpuName = Global.cpuName.Replace("(TM)", "™");
            Global.cpuName = Global.cpuName.Replace("(R)", "®");
            tbxCPUName.Text = Global.cpuName;
            tbxiGPUName.Text = GetSystemInfo.GetGPUName(0).Replace("(R)", "®");
            string dGPU = GetSystemInfo.GetGPUName(1).Replace(" GPU", null);
            if (dGPU == null || dGPU == "") spdGPU.Visibility = System.Windows.Visibility.Collapsed;
            else tbxdGPUName.Text = dGPU;

            tbxRAM.Text = GetSystemInfo.GetRAM();

            sdBattery.Value = (int)Settings.Default.BatLimit;
            sdBright.Value = (int)GetSystemInfo.getBrightness();
            SetSystemSettings.setBatteryLimit((int)sdBattery.Value);

            lblMinDisplay.Content = $" {GetSystemInfo.minRefreshRate}Hz";
            lblMaxDisplay.Content = $" {GetSystemInfo.maxRefreshRate}Hz";
            lblDisplayAuto.Content = " Auto";
            lblDisplayOver.Content = " OD  ";

            if (MotherboardInfo.Product.Contains("Flow X16"))
            {
                ugFans.Columns = 3;
                hasSysFan = true;
                gSysFan.Visibility = Visibility.Visible;
            }

            updateFan();

            GetSystemInfo.CurrentDisplayRrefresh();

            tsModernStandby.IsChecked = Settings.Default.ModernStandby;

            //set up timer for sensor update
            DispatcherTimer sensor = new DispatcherTimer();
            sensor.Interval = TimeSpan.FromSeconds(1.7);
            sensor.Tick += SensorUpdate_Tick;
            sensor.Start();

            if (Settings.Default.xgMobileLED == true)
            {
                tbLedOn.IsChecked = true;
                tbLedOff.IsChecked = false;
                xgMobileConnectionService.EnableXgMobileLight();
            }
            else
            {
                tbLedOn.IsChecked = false;
                tbLedOff.IsChecked = true;
                xgMobileConnectionService.DisableXgMobileLight();
            }

            if (Settings.Default.multiZone == true)
            {
                tbMultizone.IsChecked = true;
                App.wmi.DeviceSet(ASUSWmi.ScreenMultizone, 1);
                lblMultizone.Content = " MZ";
            }
            else
            {
                tbMultizone.IsChecked = false;
                App.wmi.DeviceSet(ASUSWmi.ScreenMultizone, 0);
                lblMultizone.Content = " SZ";
            }

            if (App.wmi.DeviceGet(ASUSWmi.ScreenMultizone) == 1)
            {
                tbMultizone.IsChecked = true;
                App.wmi.DeviceSet(ASUSWmi.ScreenMultizone, 1);
                lblMultizone.Content = " MZ";
                Settings.Default.multiZone = true;
            }

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
            App.ApplyFix();
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

            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            update();
            SetSystemSettings.setBatteryLimit((int)sdBattery.Value);
            setup = true;

            if (tbAuto.IsChecked == true && setup == true || tbDisplayAuto.IsChecked == true && setup == true)
            {
                await Task.Run(() => GetSystemInfo.getBattery());
                await Task.Run(() => SetSystemSettings.setACDCSettings());
            }

            GarbageCollection.Garbage_Collect();

        }

        void SensorUpdate_Tick(object sender, EventArgs e)
        {
            update();
        }

        int eGPU = 1;
        private async void update()
        {
            if (!Global.isMinimised)
            {
                updateFan();

                if (Global.updateGPU)
                {
                    string dGPU = await Task.Run(() => GetSystemInfo.GetGPUName(1).Replace(" GPU", null));
                    if (dGPU == "" && spdGPU.Visibility != System.Windows.Visibility.Collapsed) spdGPU.Visibility = System.Windows.Visibility.Collapsed;
                    else if (dGPU != "" && spdGPU.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        spdGPU.Visibility = System.Windows.Visibility.Visible;
                        tbxdGPUName.Text = dGPU;
                    }

                    Global.updateGPU = false;
                }
            }

            if (updateProfile == true) { switchProfile(Settings.Default.ACMode); updateProfile = false; }
        }

        private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            try
            {
                if (tbAuto.IsChecked == true && setup == true || tbDisplayAuto.IsChecked == true && setup == true)
                {
                    await Task.Run(() => GetSystemInfo.getBattery());
                    await Task.Run(() => SetSystemSettings.setACDCSettings());
                }
                SetSystemSettings.setBatteryLimit((int)sdBattery.Value);
            } catch {

            }
        }

        public async void switchProfile(int ACProfile)
        {
            Settings.Default.ACMode = ACProfile;
            Settings.Default.Save();

            var profileData = new Dictionary<int, (bool, bool, bool, bool, string, string)>()
    {
        { 0, (true, false, false, false, "\\Images\\ACProfiles\\Silent.png", "presets\\Silent.txt") },
        { 1, (false, false, true, false, "\\Images\\ACProfiles\\Bal.png", "presets\\Perf.txt") },
        { 2, (false, true, false, false, "\\Images\\ACProfiles\\Turbo.png", "presets\\Turbo.txt") },
        { 3, (false, false, false, true, "\\Images\\ACProfiles\\Windows.png", "presets\\Manual.txt") }
    };

            if (profileData.TryGetValue(ACProfile, out var profile))
            {
                if (Global.isMinimalGUI)
                {
                    tbSilent2.IsChecked = profile.Item1;
                    tbTurbo2.IsChecked = profile.Item2;
                    tbPerf2.IsChecked = profile.Item3;
                    tbMan2.IsChecked = profile.Item4;
                }
                else
                {
                    tbSilent.IsChecked = profile.Item1;
                    tbTurbo.IsChecked = profile.Item2;
                    tbPerf.IsChecked = profile.Item3;
                    tbMan.IsChecked = profile.Item4;
                    imgPerformProfile.Source = new BitmapImage(new Uri(App.location + profile.Item5));
                }

                await Task.Run(() => App.wmi.DeviceSet(ASUSWmi.PerformanceMode,
                    ACProfile == 0 ? ASUSWmi.PerformanceSilent :
                    ACProfile == 1 ? ASUSWmi.PerformanceBalanced :
                    ACProfile == 2 ? ASUSWmi.PerformanceTurbo : ASUSWmi.PerformanceBalanced));

                SetSystemSettings.ApplyPresetSettings(profile.Item6, ACProfile);
                updateXGFan(ACProfile);
            }
        }

        /**
         * 0 - disabled, 1 - enabled, -1 - unsupported
         */
        private int GetMux()
        {
            int mux = App.wmi.DeviceGet(ASUSWmi.GPUMux);
            if (mux == 0 || mux == 1)
            {
                return mux;
            }
            return -1;
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
            if (tbDisplayOver.IsChecked == true)
            {
                Settings.Default.DisplayOver = true;
                SetSystemSettings.setDisplayOver(1);
            }
            else
            {
                Settings.Default.DisplayOver = false;
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
            tbDisplayAuto.IsChecked = true;
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
                tbAuto.IsChecked = true;
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
            int mux = GetMux();
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
            int mux = GetMux();
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

            if (changeMode == 1) tbStan.IsChecked = true;
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
            if (mux > 0 || mux == -1)
            {
                if (eGPU < 1)
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

                XG_Mobile_Prompt xg = new XG_Mobile_Prompt(xgMobileConnectionService);
                xg.Show();
            }
        }

        private async void updateFan()
        {
            try {
                if (!Global.isMinimised)
                {
                    var cpuFan = App.wmi.DeviceGet(ASUSWmi.CPU_Fan);
                    var gpuFan = App.wmi.DeviceGet(ASUSWmi.GPU_Fan);

                    double maxFanCPU = GetSystemInfo.getCPUFanSpeed();
                    double maxFanGPU = GetSystemInfo.getGPUFanSpeed();

                    await Task.Run(() => GetSystemInfo.ReadSensors());

                    tbxCPUFan.Text = $"{cpuFan * 0x64} RPM";
                    tbxdGPUFan.Text = $"{gpuFan * 0x64} RPM";

                    double cpuFanPercentage = Math.Round(cpuFan / maxFanCPU);
                    double gpuFanPercentage = Math.Round(gpuFan / maxFanGPU);

                    prCPUFan.Progress = cpuFanPercentage;
                    prdGPUFan.Progress = gpuFanPercentage;

                    tbxCPUPer.Text = $"{App.wmi.DeviceGet(ASUSWmi.Temp_CPU)}°C";

                    if(App.wmi.DeviceGet(ASUSWmi.Temp_GPU) > 0) tbxdGPUPer.Text = $"{App.wmi.DeviceGet(ASUSWmi.Temp_GPU)}°C";
                    else tbxdGPUPer.Text = $"{Math.Round(gpuFan / maxFanGPU)}%";

                    if (hasSysFan)
                    {
                        var sysFan = App.wmi.DeviceGet(ASUSWmi.SYS_Fan);
                        double maxFanSYS = GetSystemInfo.getSYSFanSpeed();
                        tbxSysFan.Text = $"{sysFan * 0x64} RPM";
                        double sysFanPercentage = Math.Round(sysFan / maxFanCPU);
                        tbxSysPer.Text = $"{Math.Round(sysFan / maxFanSYS)}%";
                    }

                    if (tbxCPUFan.Text.Contains("-") || tbxdGPUFan.Text.Contains("-"))
                    {
                        cpuFan = App.wmi.DeviceGet2(ASUSWmi.CPU_Fan);
                        gpuFan = App.wmi.DeviceGet2(ASUSWmi.GPU_Fan);

                        tbxCPUFan.Text = $"{cpuFan * 0x64} RPM";
                        tbxdGPUFan.Text = $"{gpuFan * 0x64} RPM";

                        tbxdGPUPer.Text = $"{Math.Round(gpuFan / maxFanGPU)}%";

                        cpuFanPercentage = Math.Round(cpuFan / 0.69);
                        gpuFanPercentage = Math.Round(gpuFan / 0.69);

                        prCPUFan.Progress = cpuFanPercentage;
                        prdGPUFan.Progress = gpuFanPercentage;
                    }

                    float dischargeRate = (float)GetSystemInfo.BatteryDischarge;

                    if (dischargeRate != 0)
                    {
                        spDischarge.Visibility = Visibility.Visible;
                        tbxDischarge.Text = $"-{dischargeRate.ToString("0.00")} W";
                    }
                    else spDischarge.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private void OnXgMobileStatusUpdate(object? _, XgMobileConnectionService.XgMobileStatusEvent e)
        {
            Dispatcher.Invoke(() => UpdateXgMobileStatus(e.Detected, e.Connected));
        }

        private async void UpdateXgMobileStatus(bool detected, bool connected)
        {
            try
            {
                if (!detected)
                {
                    cdXGMobile.Visibility = Visibility.Collapsed;
                }
                else
                {
                    cdXGMobile.Visibility = Visibility.Visible;
                }
                eGPU = detected && connected ? 1 : 0;
                if (eGPU == 0 && lblXGMobile.Content != "Activate ROG XG Mobile")
                { lblXGMobile.Content = "Activate ROG XG Mobile"; }
                if (eGPU == 1 && lblXGMobile.Content != "Deactivate ROG XG Mobile") { lblXGMobile.Content = "Deactivate ROG XG Mobile"; }

                if (mux == 0 && tbXG.IsEnabled == true) { tbXG.IsEnabled = false; }
                else if (mux == -1)
                {
                    tbXG.IsEnabled = true;
                }
                else { tbXG.IsEnabled = true; }
            }
            catch { }
        }

        private void tbMultizone_Click(object sender, RoutedEventArgs e)
        {
            if (tbMultizone.IsChecked == true)
            {
                App.wmi.DeviceSet(ASUSWmi.ScreenMultizone, 1);
                lblMultizone.Content = " MZ";
                Settings.Default.multiZone = true;
            }
            else
            {
                App.wmi.DeviceSet(ASUSWmi.ScreenMultizone, 0);
                lblMultizone.Content = " SZ";
                Settings.Default.multiZone = false;
            }

            if (App.wmi.DeviceGet(ASUSWmi.ScreenMultizone) == 1)
            {
                tbMultizone.IsChecked = true;
                App.wmi.DeviceSet(ASUSWmi.ScreenMultizone, 1);
                lblMultizone.Content = " MZ";
                Settings.Default.multiZone = true;
            }

            Settings.Default.Save();
        }

        private void tbLedOff_Click(object sender, RoutedEventArgs e)
        {
            tbLedOn.IsChecked = false;
            tbLedOff.IsChecked = true;
            xgMobileConnectionService.DisableXgMobileLight();
            Settings.Default.xgMobileLED = false;
            Settings.Default.Save();
        }

        private void tbLedOn_Click(object sender, RoutedEventArgs e)
        {
            tbLedOn.IsChecked = true;
            tbLedOff.IsChecked = false;
            xgMobileConnectionService.EnableXgMobileLight();
            Settings.Default.xgMobileLED = true;
            Settings.Default.Save();
        }

        private async void updateXGFan(int mode)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(500);
                if (Settings.Default.ACMode == mode)
                {
                    if (eGPU == 1)
                    {
                        if (CustomPresetHandler.isXgFan == true)
                        {
                            List<CurvePoint> xgFanCurve = new List<CurvePoint>();
                            xgFanCurve.Add(new CurvePoint()
                            {
                                Temperature = 30,
                                Fan = CustomPresetHandler.xgFan1
                            });
                            xgFanCurve.Add(new CurvePoint()
                            {
                                Temperature = 40,
                                Fan = CustomPresetHandler.xgFan2
                            });
                            xgFanCurve.Add(new CurvePoint()
                            {
                                Temperature = 50,
                                Fan = CustomPresetHandler.xgFan3
                            });
                            xgFanCurve.Add(new CurvePoint()
                            {
                                Temperature = 60,
                                Fan = CustomPresetHandler.xgFan4
                            });
                            xgFanCurve.Add(new CurvePoint()
                            {
                                Temperature = 70,
                                Fan = CustomPresetHandler.xgFan5
                            });
                            xgFanCurve.Add(new CurvePoint()
                            {
                                Temperature = 80,
                                Fan = CustomPresetHandler.xgFan6
                            });
                            xgFanCurve.Add(new CurvePoint()
                            {
                                Temperature = 90,
                                Fan = CustomPresetHandler.xgFan7
                            });
                            xgFanCurve.Add(new CurvePoint()
                            {
                                Temperature = 100,
                                Fan = CustomPresetHandler.xgFan8
                            });

                            xgMobileConnectionService.SetXgMobileFan(xgFanCurve);
                        } else
                        {
                            xgMobileConnectionService.ResetXgMobileFan();
                        }
                    }
                }
            });
        }

        private void tsModernStandby_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.ModernStandby = (bool)tsModernStandby.IsChecked;
        }

        private void SizeSlider_TouchDown(object sender, TouchEventArgs e)
        {
            // Mark event as handled
            e.Handled = true;
        }
    }
}