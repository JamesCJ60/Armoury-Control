using acControl.Properties;
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
        public static string GetCPUName()
        {

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    return obj["Name"].ToString();
                }
            }
            catch (Exception ex) { }

            return "";
        }

        public static string GetGPUName(int i)
        {
            try
            {
                int count = 0;

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", $"SELECT * FROM Win32_VideoController"); // Change AdapterCompatibility as per your requirement
                ManagementObjectCollection collection = searcher.Get();

                foreach (ManagementObject obj in collection)
                {
                    if (count == i)
                    {
                        return obj["Name"].ToString();
                    }
                    count++;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }

            return "";
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
