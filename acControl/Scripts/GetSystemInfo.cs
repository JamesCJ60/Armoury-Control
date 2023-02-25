using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace acControl.Scripts
{
    internal class GetSystemInfo
    {
        public static Computer thisPC;
        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }


        public static void start()
        {
            thisPC = new Computer()
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsBatteryEnabled = true,
                IsPsuEnabled = true,
                IsControllerEnabled = true,
                IsMotherboardEnabled = true
            };
            thisPC.Open();
            thisPC.Accept(new UpdateVisitor());
        }

        public static void stop()
        {
            thisPC.Close();
        }

        public static string GetCPUName()
        {

            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.Cpu)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            return hardware.Name;
                        }
                    }
                }
            }
            catch (Exception ex) { }

            return "";
        }

        public static string GetiGPUName()
        {
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.GpuAmd)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                            {
                                if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                {
                                    return hardware.Name;
                                }
                            }

                        }
                    }

                    if (hardware.HardwareType == HardwareType.GpuIntel)
                    {
                        return hardware.Name;
                    }
                }
            }
            catch (Exception ex) { }

            return "";
        }

        public static int cpuTemp, cpuPower, cpuClock, cpuLoad;
        public static float cpuVolt;

        public static void getCPUStats()
        {
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.Cpu)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core"))
                            {
                                cpuTemp = (int)sensor.Value.GetValueOrDefault();
                            }

                            if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Package"))
                            {
                                cpuPower = (int)sensor.Value.GetValueOrDefault();
                            }

                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("1"))
                            {
                                cpuClock = (int)sensor.Value.GetValueOrDefault();
                            }

                            if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Total"))
                            {
                                cpuLoad = (int)sensor.Value.GetValueOrDefault();
                            }

                            if (sensor.SensorType == SensorType.Voltage && sensor.Name.Contains("Core"))
                            {
                                cpuVolt = sensor.Value.GetValueOrDefault() * 1000;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }


        public static string GetdGPUName()
        {
                try
                {
                    foreach (var hardware in thisPC.Hardware)
                    {
                        hardware.Update();
                        if (hardware.HardwareType == HardwareType.GpuAmd)
                        {
                            foreach (var sensor in hardware.Sensors)
                            {

                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {

                                    }
                                    else
                                    {
                                        return hardware.Name;
                                    }
                                }
                                else
                                {
                                    return hardware.Name;
                                }
                            }
                        }

                        else if (hardware.HardwareType == HardwareType.GpuNvidia)
                        {
                            foreach (var sensor in hardware.Sensors)
                            {
                                return hardware.Name;
                            }
                        }
                    }
                }
                catch (Exception ex) { return ""; }

                return "";

            return "";
        }

        public static int dGPUClock;
        public static int dGPUMemClock;
        public static int dGPUPower;
        public static int dGPUTemp;
        public static int dGPULoad;
        public static float dGPUVolt;


        public static void GetdGPUStats()
        {
            try
            {
                foreach (var hardware in thisPC.Hardware)
                {
                    hardware.Update();
                    if (hardware.HardwareType == HardwareType.GpuAmd)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Core"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {

                                    }
                                    else dGPUClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                                }
                                else dGPUClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }

                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Mem"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {

                                    }
                                    else dGPUMemClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                                }
                                else dGPUMemClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());

                            }

                            if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {


                                    }
                                    else dGPUTemp = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                                }
                                else dGPUTemp = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }

                            if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Core"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {

                                    }
                                    else dGPUPower = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                                }
                                else dGPUPower = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }

                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Mem"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {

                                    }
                                    else dGPUClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                                }
                                else dGPUClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }

                            if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core"))
                            {
                                if (!hardware.Name.Contains("56") || !hardware.Name.Contains("64"))
                                {
                                    if (hardware.Name.Contains("Vega") && hardware.Name.Contains("3") || hardware.Name.Contains("Vega") && hardware.Name.Contains("6") || hardware.Name.Contains("Vega") && hardware.Name.Contains("8") || hardware.Name.Contains("Vega") && hardware.Name.Contains("9") || hardware.Name.Contains("Vega") && hardware.Name.Contains("10") || hardware.Name.Contains("Vega") && hardware.Name.Contains("11") || hardware.Name == "AMD Radeon Graphics" || hardware.Name == "AMD Radeon(TM) Graphics" || hardware.Name == "AMD Radeon RX Vega Graphics")
                                    {

                                    }
                                    else dGPULoad = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                                }
                                else dGPULoad = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }
                        }
                    }

                    if (hardware.HardwareType == HardwareType.GpuNvidia)
                    {
                        foreach (var sensor in hardware.Sensors)
                        {
                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Core"))
                            {
                                dGPUClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }

                            if (sensor.SensorType == SensorType.Clock && sensor.Name.Contains("Mem"))
                            {
                                dGPUMemClock = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }

                            if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Core"))
                            {
                                dGPUTemp = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }

                            if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Core"))
                            {
                                dGPULoad = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }

                            if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("GPU"))
                            {
                                dGPUPower = Convert.ToInt32(sensor.Value.GetValueOrDefault());
                            }

                            if (sensor.SensorType == SensorType.Voltage)
                            {
                                dGPUVolt = sensor.Value.GetValueOrDefault() * 1000;
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }


        //create a management scope object
        public static ManagementScope scope = new ManagementScope("\\\\.\\ROOT\\WMI");

        public static int getBrightness()
        {
            try
            {
                //create object query
                ObjectQuery query = new ObjectQuery("SELECT * FROM WmiMonitorBrightness");

                //create object searcher
                ManagementObjectSearcher searcher =
                                        new ManagementObjectSearcher(scope, query);

                //get a collection of WMI objects
                ManagementObjectCollection queryCollection = searcher.Get();

                //enumerate the collection.
                foreach (ManagementObject m in queryCollection)
                {
                    // access properties of the WMI object
                    return Convert.ToInt32(m["CurrentBrightness"]);
                }

                return 0;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public static int[] HorizontalResolution = new int[100];
        public static int[] VerticalResolution = new int[100];
        public static int[] RefreshRate = new int[100];

        public static int minRefreshRate = 0;
        public static int maxRefreshRate = 0;
        public static int maxVertRes = 0;
        public static int maxHorizRes = 0;

        public static void getDisplayData()
        {
            var scope = new ManagementScope();

            var query = new ObjectQuery("SELECT * FROM CIM_VideoControllerResolution");

            using (var searcher = new ManagementObjectSearcher(scope, query))
            {
                var results = searcher.Get();


                int i = 0;
                foreach (var result in results)
                {
                    i++;
                }

                HorizontalResolution = new int[i];
                VerticalResolution = new int[i];
                RefreshRate = new int[i];

                i = 0;
                foreach (var result in results)
                {
                    HorizontalResolution[i] = Convert.ToInt32(result["HorizontalResolution"]);
                    VerticalResolution[i] = Convert.ToInt32(result["VerticalResolution"]);
                    RefreshRate[i] = Convert.ToInt32(result["RefreshRate"]);
                    i++;
                }
            }
            Array.Sort(HorizontalResolution);
            Array.Sort(VerticalResolution);
            Array.Sort(RefreshRate);

            maxHorizRes = HorizontalResolution.Max();
            maxVertRes = VerticalResolution.Max();
            maxRefreshRate = RefreshRate.Max();
            if (RefreshRate.Min() < 60) minRefreshRate = 60;
            else minRefreshRate = RefreshRate.Min();
        }

        public static int currentRefreshRate = 0;
        public static void CurrentDisplayRrefresh()
        {

                ManagementObjectSearcher searcher =
            new ManagementObjectSearcher("root\\CIMV2",
            "SELECT * FROM Win32_VideoController");
                int i = 0;
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (i < 1) currentRefreshRate = Convert.ToInt32(queryObj["CurrentRefreshRate"]);
                    i++;
                }
        }

        public static string GPUFanSpeed()
        {
            string gpuFan = RunCLI.RunCommand("Powershell.exe (Get-WmiObject -Namespace root/WMI -Class AsusAtkWmi_WMNB).DSTS(0x00110014)", true);
            var result = gpuFan.Split('\n');
            string output = result[12].Substring(result[12].IndexOf(':') + 2);
            int gpuSpeed = Convert.ToInt32(output);
            string hexValue = gpuSpeed.ToString("X");

            uint fanSpeed = Convert.ToUInt32(hexValue, 16);
            fanSpeed = (fanSpeed - 0x00010000) * 0x64;
            return fanSpeed.ToString();
        }

        public static string CPUFanSpeed()
        {
            string cpuFan = RunCLI.RunCommand("Powershell.exe (Get-WmiObject -Namespace root/WMI -Class AsusAtkWmi_WMNB).DSTS(0x00110013)", true);
            var result = cpuFan.Split('\n');
            string output = result[12].Substring(result[12].IndexOf(':') + 2);
            int cpuSpeed = Convert.ToInt32(output);
            string hexValue = cpuSpeed.ToString("X");

            uint fanSpeed = Convert.ToUInt32(hexValue, 16);
            fanSpeed = (fanSpeed - 0x00010000) * 0x64;
            return fanSpeed.ToString();
        }

        public static string batPercent = "";
        public static int batPercentInt = 0;
        public static UInt16 statuscode = 9999;

        //Pull battery sensor info from Windows
        public async static void getBattery()
        {
            await Task.Run(() =>
            {
                int batteryLife = 0;
                try
                {
                    ManagementClass wmi = new ManagementClass("Win32_Battery");
                    ManagementObjectCollection allBatteries = wmi.GetInstances();

                    double batteryLevel = 0;

                    //Get battery level from each system battery detected
                    foreach (var battery in allBatteries)
                    {
                        batteryLevel = Convert.ToDouble(battery["EstimatedChargeRemaining"]);
                        statuscode = (UInt16)battery["BatteryStatus"];
                    }
                    //Set battery level as an int
                    batteryLife = (int)batteryLevel;
                    batPercentInt = batteryLife;

                    //Update battery level string
                    batPercent = batteryLife.ToString() + "%";
                }
                catch (Exception ex)
                {

                }
            });
        }
    }

}
