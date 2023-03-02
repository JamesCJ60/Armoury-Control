using acControl.Properties;
using acControl.Scripts;
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
using System.Windows.Threading;

namespace acControl.Views.Pages
{
    /// <summary>
    /// Interaction logic for CustomPresets.xaml
    /// </summary>
    public partial class CustomPresets : Page
    {
        public CustomPresets()
        {
            InitializeComponent();
            _ = Tablet.TabletDevices;

            string preset = "presets\\Manual.txt";

            cbxPowerPreset.SelectedIndex = Settings.Default.ACMode;

            try
            {
                if (cbxPowerPreset.SelectedIndex == 0) preset = "presets\\Silent.txt";
                if (cbxPowerPreset.SelectedIndex == 1) preset = "presets\\Perf.txt";
                if (cbxPowerPreset.SelectedIndex == 2) preset = "presets\\Turbo.txt";
                CustomPresetHandler.LoadPreset(preset);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }

            if (GetSystemInfo.GetCPUName().Contains("Intel"))
            {
                spAPUCO.Visibility = Visibility.Collapsed;
                spAPUSlow.Visibility = Visibility.Collapsed;
                sdThermal.Visibility = Visibility.Collapsed;
                tbSmart.Visibility = Visibility.Collapsed;
            }
        }

        async void loadSettings(string preset)
        {
            try
            {
                await Task.Run(() => CustomPresetHandler.LoadPreset(preset));

                tsCPUFan.IsChecked = CustomPresetHandler.isCPUFan;
                tsCPUPower.IsChecked = CustomPresetHandler.isCPUPower;
                tsCPUTemp.IsChecked = CustomPresetHandler.isCPUTemp;

                tsGPUFan.IsChecked = CustomPresetHandler.isGPUFan;
                tsGPUOffset.IsChecked = CustomPresetHandler.isGPUOffset;


                nudCPUTemp1.Value = CustomPresetHandler.cpuTemp;
                nudCPUTemp2.Value = CustomPresetHandler.skinCPUTemp;

                nudAPUCO.Value = CustomPresetHandler.cpuCurveOpti;

                if (CustomPresetHandler.cpuPower1 == 0)
                {
                    if (GetSystemInfo.GetCPUName().Contains("Ryzen"))
                    {
                        if (App.wmi.DeviceGet(ASUSWmi.PPT_Total) > 0) nudCPUPow1.Value = (int)App.wmi.DeviceGet(ASUSWmi.PPT_Total);
                        else if (App.wmi.DeviceGet(ASUSWmi.PPT_Total1) > 0) nudCPUPow1.Value = (int)App.wmi.DeviceGet(ASUSWmi.PPT_Total1);
                        else nudCPUPow1.Value = 65;
                    }
                    else nudCPUPow1.Value = 65;
                }

                if (CustomPresetHandler.cpuPower2 == 0)
                {
                    if (GetSystemInfo.GetCPUName().Contains("Ryzen"))
                    {
                        if (App.wmi.DeviceGet(ASUSWmi.PPT_Total) > 0) nudCPUPow2.Value = (int)App.wmi.DeviceGet(ASUSWmi.PPT_Total);
                        else if (App.wmi.DeviceGet(ASUSWmi.PPT_Total1) > 0) nudCPUPow2.Value = (int)App.wmi.DeviceGet(ASUSWmi.PPT_Total1);
                        else if (App.wmi.DeviceGet(ASUSWmi.PPT_Total2) > 0) nudCPUPow2.Value = (int)App.wmi.DeviceGet(ASUSWmi.PPT_Total2);
                        else nudCPUPow2.Value = 65;
                    }
                    else nudCPUPow2.Value = 65;
                }

                if (CustomPresetHandler.apuSlowPPT == 0)
                {
                    if (GetSystemInfo.GetCPUName().Contains("Ryzen"))
                    {
                        if (App.wmi.DeviceGet(ASUSWmi.PPT_CPU) > 0) nudAPUPow.Value = (int)App.wmi.DeviceGet(ASUSWmi.PPT_CPU1);
                        else if (App.wmi.DeviceGet(ASUSWmi.PPT_CPU1) > 0) nudAPUPow.Value = (int)App.wmi.DeviceGet(ASUSWmi.PPT_CPU1);
                        else nudCPUPow2.Value = 45;
                    }
                    else nudCPUPow2.Value = 45;
                }

                sdCPUFan1.Value = CustomPresetHandler.cpuFan1;
                sdCPUFan2.Value = CustomPresetHandler.cpuFan2;
                sdCPUFan3.Value = CustomPresetHandler.cpuFan3;
                sdCPUFan4.Value = CustomPresetHandler.cpuFan4;
                sdCPUFan5.Value = CustomPresetHandler.cpuFan5;
                sdCPUFan6.Value = CustomPresetHandler.cpuFan6;
                sdCPUFan7.Value = CustomPresetHandler.cpuFan7;
                sdCPUFan8.Value = CustomPresetHandler.cpuFan8;

                nudGPUCore.Value = CustomPresetHandler.gpuCoreOffset;
                nudGPUVRAM.Value = CustomPresetHandler.gpuVRAMOffset;

                sdGPUFan1.Value = CustomPresetHandler.gpuFan1;
                sdGPUFan2.Value = CustomPresetHandler.gpuFan2;
                sdGPUFan3.Value = CustomPresetHandler.gpuFan3;
                sdGPUFan4.Value = CustomPresetHandler.gpuFan4;
                sdGPUFan5.Value = CustomPresetHandler.gpuFan5;
                sdGPUFan6.Value = CustomPresetHandler.gpuFan6;
                sdGPUFan7.Value = CustomPresetHandler.gpuFan7;
                sdGPUFan8.Value = CustomPresetHandler.gpuFan8;

            } catch (Exception ex) { }
        }

        private void save(string preset = "presets\\Silent.txt")
        {
            CustomPresetHandler.isCPUFan = tsCPUFan.IsChecked.Value;
            CustomPresetHandler.isCPUPower = tsCPUPower.IsChecked.Value;
            CustomPresetHandler.isCPUTemp = tsCPUTemp.IsChecked.Value;

            CustomPresetHandler.isGPUFan = tsGPUFan.IsChecked.Value;
            CustomPresetHandler.isGPUOffset = tsGPUOffset.IsChecked.Value;

            CustomPresetHandler.cpuTemp = (int)nudCPUTemp1.Value;
            CustomPresetHandler.skinCPUTemp = (int)nudCPUTemp2.Value;

            CustomPresetHandler.cpuPower1 = (int)nudCPUPow1.Value;
            CustomPresetHandler.cpuPower2 = (int)nudCPUPow2.Value;
            CustomPresetHandler.apuSlowPPT = (int)nudAPUPow.Value;
            CustomPresetHandler.cpuCurveOpti = (int)nudAPUCO.Value;

            CustomPresetHandler.cpuFan1 = (int)sdCPUFan1.Value;
            CustomPresetHandler.cpuFan2 = (int)sdCPUFan2.Value;
            CustomPresetHandler.cpuFan3 = (int)sdCPUFan3.Value;
            CustomPresetHandler.cpuFan4 = (int)sdCPUFan4.Value;
            CustomPresetHandler.cpuFan5 = (int)sdCPUFan5.Value;
            CustomPresetHandler.cpuFan6 = (int)sdCPUFan6.Value;
            CustomPresetHandler.cpuFan7 = (int)sdCPUFan7.Value;
            CustomPresetHandler.cpuFan8 = (int)sdCPUFan8.Value;

            if ((int)sdCPUFan5.Value < 30) CustomPresetHandler.cpuFan5 = 30;
            if ((int)sdCPUFan6.Value < 30) CustomPresetHandler.cpuFan6 = 30;
            if ((int)sdCPUFan7.Value < 30) CustomPresetHandler.cpuFan7 = 30;
            if ((int)sdCPUFan8.Value < 30) CustomPresetHandler.cpuFan8 = 30;

            CustomPresetHandler.gpuCoreOffset = (int)nudGPUCore.Value;
            CustomPresetHandler.gpuVRAMOffset = (int)nudGPUVRAM.Value;

            CustomPresetHandler.gpuFan1 = (int)sdGPUFan1.Value;
            CustomPresetHandler.gpuFan2 = (int)sdGPUFan2.Value;
            CustomPresetHandler.gpuFan3 = (int)sdGPUFan3.Value;
            CustomPresetHandler.gpuFan4 = (int)sdGPUFan4.Value;
            CustomPresetHandler.gpuFan5 = (int)sdGPUFan5.Value;
            CustomPresetHandler.gpuFan6 = (int)sdGPUFan6.Value;
            CustomPresetHandler.gpuFan7 = (int)sdGPUFan7.Value;
            CustomPresetHandler.gpuFan8 = (int)sdGPUFan8.Value;

            if ((int)sdGPUFan5.Value < 30) CustomPresetHandler.gpuFan5 = 30;
            if ((int)sdGPUFan6.Value < 30) CustomPresetHandler.gpuFan6 = 30;
            if ((int)sdGPUFan7.Value < 30) CustomPresetHandler.gpuFan7 = 30;
            if ((int)sdGPUFan8.Value < 30) CustomPresetHandler.gpuFan8 = 30;

            CustomPresetHandler.SavePreset(preset);
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            save();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            string preset = "presets\\Manual.txt";
            if (cbxPowerPreset.SelectedIndex == 0) preset = "presets\\Silent.txt";
            if (cbxPowerPreset.SelectedIndex == 1) preset = "presets\\Perf.txt";
            if (cbxPowerPreset.SelectedIndex == 2) preset = "presets\\Turbo.txt";
            loadSettings(preset);
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            string preset = "\\presets\\Manual.txt";
            if (cbxPowerPreset.SelectedIndex == 0) preset = "presets\\Silent.txt";
            if (cbxPowerPreset.SelectedIndex == 1) preset = "presets\\Perf.txt";
            if (cbxPowerPreset.SelectedIndex == 2) preset = "presets\\Turbo.txt";

            save(preset);
            Settings.Default.ACMode = cbxPowerPreset.SelectedIndex;
            Settings.Default.Save();
            DashboardPage.updateProfile = true;
        }

        private void cbxPowerPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string preset = "presets\\Manual.txt";
            if (cbxPowerPreset.SelectedIndex == 0) preset = "presets\\Silent.txt";
            if (cbxPowerPreset.SelectedIndex == 1) preset = "presets\\Perf.txt";
            if (cbxPowerPreset.SelectedIndex == 2) preset = "presets\\Turbo.txt";
            loadSettings(preset);
        }
    }
}
