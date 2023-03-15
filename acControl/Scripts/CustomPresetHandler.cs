using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace acControl.Scripts
{
    class CustomPresetHandler
    {
        public static bool isCPUTemp = false;
        public static bool isCPUPower = false;
        public static bool isCPUFan = false;

        public static bool isGPUOffset = false;
        public static bool isGPUFan = false;

        public static bool isSYSFan = false;

        public static bool isXgFan = false;

        public static int cpuTemp = 95;
        public static int skinCPUTemp = 45;

        public static int cpuPower1 = 45;
        public static int cpuPower2 = 45;
        public static int apuSlowPPT = 45;
        public static int cpuCurveOpti = 0;

        public static int cpuFan1 = 30;
        public static int cpuFan2 = 30;
        public static int cpuFan3 = 30;
        public static int cpuFan4 = 30;
        public static int cpuFan5 = 30;
        public static int cpuFan6 = 30;
        public static int cpuFan7 = 30;
        public static int cpuFan8 = 30;

        public static int gpuCoreOffset = 0;
        public static int gpuVRAMOffset = 0;

        public static int gpuFan1 = 30;
        public static int gpuFan2 = 30;
        public static int gpuFan3 = 30;
        public static int gpuFan4 = 30;
        public static int gpuFan5 = 30;
        public static int gpuFan6 = 30;
        public static int gpuFan7 = 30;
        public static int gpuFan8 = 30;

        public static int sysFan1 = 30;
        public static int sysFan2 = 30;
        public static int sysFan3 = 30;
        public static int sysFan4 = 30;
        public static int sysFan5 = 30;
        public static int sysFan6 = 30;
        public static int sysFan7 = 30;
        public static int sysFan8 = 30;

        public static int xgFan1 = 30;
        public static int xgFan2 = 30;
        public static int xgFan3 = 30;
        public static int xgFan4 = 30;
        public static int xgFan5 = 30;
        public static int xgFan6 = 30;
        public static int xgFan7 = 30;
        public static int xgFan8 = 30;

        public static async void LoadPreset(string preset)
        {
            await Task.Run(() =>
            {
                try
                {
                    var presetData = File.ReadAllLines(App.location + preset);

                    isCPUFan = Convert.ToBoolean(presetData[1]);
                    cpuFan1 = Convert.ToInt32(presetData[2]);
                    cpuFan2 = Convert.ToInt32(presetData[3]);
                    cpuFan3 = Convert.ToInt32(presetData[4]);
                    cpuFan4 = Convert.ToInt32(presetData[5]);
                    cpuFan5 = Convert.ToInt32(presetData[6]);
                    cpuFan6 = Convert.ToInt32(presetData[7]);
                    cpuFan7 = Convert.ToInt32(presetData[8]);
                    cpuFan8 = Convert.ToInt32(presetData[9]);

                    isGPUFan = Convert.ToBoolean(presetData[12]);
                    gpuFan1 = Convert.ToInt32(presetData[13]);
                    gpuFan2 = Convert.ToInt32(presetData[14]);
                    gpuFan3 = Convert.ToInt32(presetData[15]);
                    gpuFan4 = Convert.ToInt32(presetData[16]);
                    gpuFan5 = Convert.ToInt32(presetData[17]);
                    gpuFan6 = Convert.ToInt32(presetData[18]);
                    gpuFan7 = Convert.ToInt32(presetData[19]);
                    gpuFan8 = Convert.ToInt32(presetData[20]);

                    isGPUOffset = Convert.ToBoolean(presetData[23]);
                    gpuCoreOffset = Convert.ToInt32(presetData[24]);
                    gpuVRAMOffset = Convert.ToInt32(presetData[25]);

                    isCPUPower = Convert.ToBoolean(presetData[31]);
                    cpuPower1 = Convert.ToInt32(presetData[34]);
                    cpuPower2 = Convert.ToInt32(presetData[37]);
                    apuSlowPPT = Convert.ToInt32(presetData[40]);

                    isCPUTemp = Convert.ToBoolean(presetData[43]);
                    cpuTemp = Convert.ToInt32(presetData[44]);
                    skinCPUTemp = Convert.ToInt32(presetData[45]);

                    isSYSFan = Convert.ToBoolean(presetData[48]);
                    sysFan1 = Convert.ToInt32(presetData[49]);
                    sysFan2 = Convert.ToInt32(presetData[50]);
                    sysFan3 = Convert.ToInt32(presetData[51]);
                    sysFan4 = Convert.ToInt32(presetData[52]);
                    sysFan5 = Convert.ToInt32(presetData[53]);
                    sysFan6 = Convert.ToInt32(presetData[54]);
                    sysFan7 = Convert.ToInt32(presetData[55]);
                    sysFan8 = Convert.ToInt32(presetData[56]);

                    isXgFan = Convert.ToBoolean(presetData[59]);
                    xgFan1 = Convert.ToInt32(presetData[60]);
                    xgFan2 = Convert.ToInt32(presetData[61]);
                    xgFan3 = Convert.ToInt32(presetData[62]);
                    xgFan4 = Convert.ToInt32(presetData[63]);
                    xgFan5 = Convert.ToInt32(presetData[64]);
                    xgFan6 = Convert.ToInt32(presetData[65]);
                    xgFan7 = Convert.ToInt32(presetData[66]);
                    xgFan8 = Convert.ToInt32(presetData[67]);
                }
                catch { return; }
            });
        }

        public static async void SavePreset(string preset)
        {
            await Task.Run(() =>
            {
                string presetData = "CPU:\n";
                presetData = presetData + $"{isCPUFan}\n";
                presetData = presetData + $"{cpuFan1}\n{cpuFan2}\n{cpuFan3}\n{cpuFan4}\n{cpuFan5}\n{cpuFan6}\n{cpuFan7}\n{cpuFan8}\n\n";

                presetData = presetData + "GPU:\n";
                presetData = presetData + $"{isGPUFan}\n";
                presetData = presetData + $"{gpuFan1}\n{gpuFan2}\n{gpuFan3}\n{gpuFan4}\n{gpuFan5}\n{gpuFan6}\n{gpuFan7}\n{gpuFan8}\n\n";

                presetData = presetData + "GPU Clock Offsets:\n";
                presetData = presetData + $"{isGPUOffset}\n";
                presetData = presetData + $"{gpuCoreOffset}\n";
                presetData = presetData + $"{gpuVRAMOffset}\n\n";

                presetData = presetData + "Curve Optimiser Offsets:\n";
                presetData = presetData + $"{cpuCurveOpti}\n\n";

                presetData = presetData + "Power Limit:\n";
                presetData = presetData + $"{isCPUPower}\n\n";

                presetData = presetData + "Power Limit 1:\n";
                presetData = presetData + $"{cpuPower1}\n\n";

                presetData = presetData + "Power Limit 2:\n";
                presetData = presetData + $"{cpuPower2}\n\n";

                presetData = presetData + "APU PPT:\n";
                presetData = presetData + $"{apuSlowPPT}\n\n";

                presetData = presetData + "Temp Limits:\n";
                presetData = presetData + $"{isCPUTemp}\n";
                presetData = presetData + $"{cpuTemp}\n";
                presetData = presetData + $"{skinCPUTemp}\n\n";

                presetData = presetData + "SYS:\n";
                presetData = presetData + $"{isSYSFan}\n";
                presetData = presetData + $"{sysFan1}\n{sysFan2}\n{sysFan3}\n{sysFan4}\n{sysFan5}\n{sysFan6}\n{sysFan7}\n{sysFan8}\n\n";

                presetData = presetData + "XG Mobile:\n";
                presetData = presetData + $"{isXgFan}\n";
                presetData = presetData + $"{xgFan1}\n{xgFan2}\n{xgFan3}\n{xgFan4}\n{xgFan5}\n{xgFan6}\n{xgFan7}\n{xgFan8}";

                try
                {
                    File.WriteAllText(App.location + preset, presetData);
                }
                catch (Exception e) { MessageBox.Show(e.Message); }
            });
        }
    }
}
