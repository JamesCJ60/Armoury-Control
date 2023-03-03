using acControl.Properties;
using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                dGPUClock = 0;
                dGPUMemClock = 0;
                dGPUPower = 0;
                dGPUTemp = 0;
                dGPULoad = 0;
                dGPUVolt = 0;

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

        public static int minRefreshRate = 60;
        public static int maxRefreshRate = 60;
        public static int maxVertRes = 0;
        public static int maxHorizRes = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public short dmOrientation;
            public short dmPaperSize;
            public short dmPaperLength;
            public short dmPaperWidth;
            public short dmScale;
            public short dmCopies;
            public short dmDefaultSource;
            public short dmPrintQuality;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll")]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        public static void getDisplayData()
        {
            DEVMODE devMode = new DEVMODE();
            EnumDisplaySettings("\\\\.\\DISPLAY1", -1, ref devMode);

            maxRefreshRate = devMode.dmDisplayFrequency;

            if (Settings.Default.MaxRefreshRate > maxRefreshRate) maxRefreshRate = Settings.Default.MaxRefreshRate;
            else if (maxRefreshRate > Settings.Default.MaxRefreshRate) Settings.Default.MaxRefreshRate = maxRefreshRate;

            Settings.Default.Save();
        }

        public static int currentRefreshRate = 0;
        public static void CurrentDisplayRrefresh()
        {
            DEVMODE devMode = new DEVMODE();
            EnumDisplaySettings("\\\\.\\DISPLAY1", -1, ref devMode);
            currentRefreshRate = devMode.dmDisplayFrequency;
        }

        public static float? CpuTemp { get; private set; }
        public static float? BatteryDischarge { get; private set; }
        public static void ReadSensors()
        {
            try
            {
                using (var ct = new PerformanceCounter("Thermal Zone Information", "Temperature", @"\_TZ.THRM", true))
                {
                    CpuTemp = ct.NextValue() - 273.15f;
                }

                using (var cb = new PerformanceCounter("Power Meter", "Power", "Power Meter (0)", true))
                    BatteryDischarge = cb.NextValue() / 1000;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed reading sensors: {ex.Message}");
            }
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

        public static double getCPUFanSpeed()
        {
            double maxFanCPU = 0.6;
            if (MotherboardInfo.Product.Contains("Flow Z13"))
            {
                maxFanCPU = 0.69;
            }

            return maxFanCPU;
        }

        public static double getSYSFanSpeed()
        {
            double maxFanCPU = 0.6;

            return maxFanCPU;
        }

        public static double getGPUFanSpeed()
        {
            double maxFanGPU = 0.6;
            if (MotherboardInfo.Product.Contains("Flow Z13"))
            {
                maxFanGPU = 0.69;
            }

            return maxFanGPU;
        }
    }

}
