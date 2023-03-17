using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using acControl;
using acControl.Models;
using acControl.Properties;
using acControl.Scripts.Intel;
using acControl.Services;
using acControl.Views.Pages;
using acControl.Views.Windows;
using HidSharp;


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
                Thread.Sleep(256);
                GetSystemInfo.getBattery();
                statuscode = GetSystemInfo.statuscode;


                if (statuscode != lastStatus)
                {
                    lastStatus = statuscode;
                    hasToggledGPU = false;
                    hasToggledDisplay = false;

                }

                if (statuscode == 2 || statuscode == 6 || statuscode == 7 || statuscode == 8)
                {
                    if (Global.toggleDisplay == true && hasToggledDisplay == false)
                    {
                        setDisplaySettings(1);
                        hasToggledDisplay = true;
                    }
                    if (Global.toggleGPU == true && hasToggledGPU == false)
                    {
                        try { if (App.wmi.DeviceGet(ASUSWmi.GPUMux) < 1) return; }
                        catch { }
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
                        try { if (App.wmi.DeviceGet(ASUSWmi.GPUMux) < 1) return; }
                        catch { }
                        setGPUSettings(1);
                        hasToggledGPU = true;
                    }
                }
            });
        }

        public static async void setGPUSettings(int index)
        {
            await Task.Run(() =>
            {
                App.wmi.DeviceSet(ASUSWmi.GPUEco, index);

                Global.updateGPU = true;
            });
        }

        public static async void setDisplaySettings(int index)
        {
            await Task.Run(() =>
            {
                //if (index == 1) RunCLI.RunCommand($"{App.location + "\\Assets\\CRS\\CSR.exe"} /f={GetSystemInfo.maxRefreshRate} /force", false);
                //else RunCLI.RunCommand($"{App.location + "\\Assets\\CRS\\CSR.exe"} /f={GetSystemInfo.minRefreshRate} /force", false);

                if (index == 1) NativeMethods.SetRefreshRate(GetSystemInfo.maxRefreshRate);
                if (index == 0) NativeMethods.SetRefreshRate(60);

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
            });
        }

        public static void setDisplayOver(int overdrive = 0)
        {
            try
            {
                App.wmi.DeviceSet(ASUSWmi.ScreenOverdrive, overdrive);
            }
            catch
            {
                Debug.WriteLine("Screen Overdrive not supported");
            }
        }

        public static async void ApplyPresetSettings(string preset, int mode)
        {
            await Task.Run(() =>
            {
                CustomPresetHandler.LoadPreset(preset);
                Thread.Sleep(500);
                if (Settings.Default.ACMode == mode)
                {
                    if (GetSystemInfo.GetCPUName().Contains("Intel"))
                    {
                        if (CustomPresetHandler.isCPUPower == true) ChangeTDP.changeTDP(CustomPresetHandler.cpuPower1, CustomPresetHandler.cpuPower2);
                    }
                    else
                    {
                        string RyzenAdj = null;
                        if (CustomPresetHandler.isCPUTemp == true) RyzenAdj = RyzenAdj + $"--tctl-temp={CustomPresetHandler.cpuTemp} --apu-skin-temp={CustomPresetHandler.skinCPUTemp} ";
                        if (CustomPresetHandler.isCPUPower == true) RyzenAdj = RyzenAdj + $"--stapm-limit={CustomPresetHandler.cpuPower1 * 1000} --fast-limit={CustomPresetHandler.cpuPower2 * 1000} --slow-limit={CustomPresetHandler.cpuPower2 * 1000}  --apu-slow-limit={CustomPresetHandler.apuSlowPPT * 1000} --vrm-current={(CustomPresetHandler.cpuPower2 * 1.33) * 1000} --vrmmax-current={(CustomPresetHandler.cpuPower2 * 1.33) * 1000}  --set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)CustomPresetHandler.cpuCurveOpti))}";
                        if (RyzenAdj != null || RyzenAdj != "") RunCLI.RunCommand($"{App.location + "\\Assets\\AMD\\ryzenadj.exe"} {RyzenAdj}", false);
                    }

                    if (CustomPresetHandler.isCPUFan == true)
                    {
                        byte[] curve = new byte[16];
                        curve[0] = (byte)30;
                        curve[1] = (byte)40;
                        curve[2] = (byte)50;
                        curve[3] = (byte)60;
                        curve[4] = (byte)70;
                        curve[5] = (byte)80;
                        curve[6] = (byte)90;
                        curve[7] = (byte)100;
                        curve[8] = (byte)CustomPresetHandler.cpuFan1;
                        curve[9] = (byte)CustomPresetHandler.cpuFan2;
                        curve[10] = (byte)CustomPresetHandler.cpuFan3;
                        curve[11] = (byte)CustomPresetHandler.cpuFan4;
                        curve[12] = (byte)CustomPresetHandler.cpuFan5;
                        curve[13] = (byte)CustomPresetHandler.cpuFan6;
                        curve[14] = (byte)CustomPresetHandler.cpuFan7;
                        curve[15] = (byte)CustomPresetHandler.cpuFan8;
                        string bitCurve = BitConverter.ToString(curve);
                        Debug.WriteLine(bitCurve);

                        App.wmi.SetFanCurve(0, curve);
                    }

                    if (CustomPresetHandler.isGPUFan == true)
                    {
                        byte[] curve = new byte[16];
                        curve[0] = (byte)30;
                        curve[1] = (byte)40;
                        curve[2] = (byte)50;
                        curve[3] = (byte)60;
                        curve[4] = (byte)70;
                        curve[5] = (byte)80;
                        curve[6] = (byte)90;
                        curve[7] = (byte)100;
                        curve[8] = (byte)CustomPresetHandler.gpuFan1;
                        curve[9] = (byte)CustomPresetHandler.gpuFan2;
                        curve[10] = (byte)CustomPresetHandler.gpuFan3;
                        curve[11] = (byte)CustomPresetHandler.gpuFan4;
                        curve[12] = (byte)CustomPresetHandler.gpuFan5;
                        curve[13] = (byte)CustomPresetHandler.gpuFan6;
                        curve[14] = (byte)CustomPresetHandler.gpuFan7;
                        curve[15] = (byte)CustomPresetHandler.gpuFan8;
                        string bitCurve = BitConverter.ToString(curve);
                        Debug.WriteLine(bitCurve);

                        App.wmi.SetFanCurve(1, curve);
                    }

                    if (CustomPresetHandler.isGPUOffset == true)
                    {
                        RunCLI.RunCommand($"{App.location + "\\Assets\\NVIDIA\\oc.exe"} 0 {CustomPresetHandler.gpuCoreOffset} {CustomPresetHandler.gpuVRAMOffset}", false);
                        RunCLI.RunCommand($"{App.location + "\\Assets\\NVIDIA\\oc.exe"} 1 {CustomPresetHandler.gpuCoreOffset} {CustomPresetHandler.gpuVRAMOffset}", false);
                    }

                    if (CustomPresetHandler.isSYSFan == true)
                    {
                        byte[] curve = new byte[16];
                        curve[0] = (byte)30;
                        curve[1] = (byte)40;
                        curve[2] = (byte)50;
                        curve[3] = (byte)60;
                        curve[4] = (byte)70;
                        curve[5] = (byte)80;
                        curve[6] = (byte)90;
                        curve[7] = (byte)100;
                        curve[8] = (byte)CustomPresetHandler.sysFan1;
                        curve[9] = (byte)CustomPresetHandler.sysFan2;
                        curve[10] = (byte)CustomPresetHandler.sysFan3;
                        curve[11] = (byte)CustomPresetHandler.sysFan4;
                        curve[12] = (byte)CustomPresetHandler.sysFan5;
                        curve[13] = (byte)CustomPresetHandler.sysFan6;
                        curve[14] = (byte)CustomPresetHandler.sysFan7;
                        curve[15] = (byte)CustomPresetHandler.sysFan8;
                        string bitCurve = BitConverter.ToString(curve);
                        Debug.WriteLine(bitCurve);

                        App.wmi.SetFanCurve(1, curve);
                    }
                }
            });
        }
    }
}
