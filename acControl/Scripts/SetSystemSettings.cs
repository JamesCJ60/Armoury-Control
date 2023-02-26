using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using acControl;
using acControl.Properties;

namespace acControl.Scripts
{
    internal class SetSystemSettings
    {
        public static async void setBatteryLimit(int batLimit)
        {
            string value = ((int)batLimit).ToString();

            Settings.Default.BatLimit = (int)batLimit;
            Settings.Default.Save();

            if (batLimit >= 50 && Convert.ToInt32(value) >= 50) await Task.Run(() => RunCLI.RunPowerShellCommand($"Powershell.exe (Get-WmiObject -Namespace root/WMI -Class AsusAtkWmi_WMNB).DEVS(0x00120057, {value})", false));
        }

        public static async void setDisplayBrightness(int newBrightness)
        {
            await Task.Run(() =>
            {
                var mclass = new ManagementClass("WmiMonitorBrightnessMethods")
                {
                    Scope = new ManagementScope(@"\\.\root\wmi")
                };
                var instances = mclass.GetInstances();
                var args = new object[] { 1, newBrightness };
                foreach (ManagementObject instance in instances)
                {
                    instance.InvokeMethod("WmiSetBrightness", args);
                }
                return;
            });
        }

        public static async void setACDCSettings()
        {
            await Task.Run(() =>
            {
                bool isBattery = false;
                UInt16 statuscode = GetSystemInfo.statuscode;
                if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8) isBattery = true;

                int eco = App.wmi.DeviceGet(ASUSWmi.GPUEco);
                int mux = App.wmi.DeviceGet(ASUSWmi.GPUMux);

                if (mux < 1) return;

                if (isBattery == true && eco == 0)
                {
                    if (Settings.Default.GPUMode == 3) setGPUSettings(1);
                    if (Settings.Default.DisplayMode == 2) setDisplaySettings(0);
                }
                if (isBattery == false && eco == 1)
                {
                    if (Settings.Default.GPUMode == 3) setDisplaySettings(0);
                    if (Settings.Default.DisplayMode == 2) setGPUSettings(1);
                }
            });
        }

        public static async void setGPUSettings(int index)
        {
            await Task.Run(() =>
            {
                App.wmi.DeviceSet(ASUSWmi.GPUEco, index);

                Thread.Sleep(1000);

                GetSystemInfo.stop();

                Thread.Sleep(1000);
                GetSystemInfo.start();

                GarbageCollection.Garbage_Collect();
            });
        }

        public static async void setDisplaySettings(int index)
        {
            await Task.Run(() =>
            {
                if (index == 1) RunCLI.RunCommand($"{App.location + "\\Assets\\CRS\\CSR.exe"} /f={GetSystemInfo.maxRefreshRate} /force", false);
                else RunCLI.RunCommand($"{App.location + "\\Assets\\CRS\\CSR.exe"} /f={GetSystemInfo.minRefreshRate} /force", false);

                try
                {
                    int overdrive = 0;
                    if (Settings.Default.DisplayOver == true) overdrive = 1;
                    if (overdrive > 0) App.wmi.DeviceSet(ASUSWmi.ScreenOverdrive, overdrive);
                }
                catch
                {
                    Debug.WriteLine("Screen Overdrive not supported");
                }

                GarbageCollection.Garbage_Collect();
            });
        }
    }
}
