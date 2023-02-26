using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using acControl;
using acControl.Properties;
using acControl.Views.Pages;

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

        public static bool hasToggledDisplay = false;
        public static bool hasToggledGPU = false;
        public static UInt16 lastStatus = 0;
        public static UInt16 statuscode = 0;

        public static async void setACDCSettings()
        {
            await Task.Run(() =>
            {
                if (App.wmi.DeviceGet(ASUSWmi.GPUMux) < 1) return;

                GetSystemInfo.getBattery();
                statuscode = GetSystemInfo.statuscode;
                if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8)
                {
                    if (Global.toggleDisplay == true && hasToggledDisplay == false)
                    {
                        setDisplaySettings(1);
                        hasToggledDisplay = true;
                    }
                    if (Global.toggleGPU == true && hasToggledGPU == false)
                    {
                        setGPUSettings(0);
                        hasToggledGPU = true;
                    }
                }
                else
                {
                    if (Global.toggleDisplay == true && hasToggledDisplay == false)
                    {
                        setDisplaySettings(0);
                        hasToggledDisplay = true;
                    }
                    if (Global.toggleGPU == true && hasToggledGPU == false)
                    {
                        setGPUSettings(1);
                        hasToggledGPU = true;
                    }
                }

                if (statuscode != lastStatus)
                {
                    lastStatus = statuscode;
                    hasToggledGPU = false;
                    hasToggledDisplay = false;

                }
            });
        }

        public static async void setGPUSettings(int index)
        {
            await Task.Run(() =>
            {
                if (App.wmi.DeviceGet(ASUSWmi.GPUEco) != index)
                {
                    App.wmi.DeviceSet(ASUSWmi.GPUEco, index);
                    if (index == 1)
                    {
                        Thread.Sleep(1000);

                        GetSystemInfo.stop();

                        Thread.Sleep(1000);
                        GetSystemInfo.start();

                        GarbageCollection.Garbage_Collect();
                    }
                }
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
