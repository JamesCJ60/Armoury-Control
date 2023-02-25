using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace acControl.Scripts
{
    public static class RunCLI
    {

        public static string RunCommand(string arguments, bool readOutput, string processName = "cmd.exe")
        {

            //Runs CLI, if readOutput is true then returns output
            try
            {

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = false;
                if (readOutput) { startInfo.RedirectStandardOutput = true; } else { startInfo.RedirectStandardOutput = false; }
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = processName;
                startInfo.Arguments = "/c " + arguments;
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;
                startInfo.CreateNoWindow = true;
                process.Start();
                if (readOutput)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    return output;

                }
                else
                {
                    process.WaitForExit();
                    return "COMPLETE";
                }


            }
            catch (Exception ex)
            {
                return "Error running CLI: " + ex.Message + " " + arguments;
            }


        }

        public static string RunPowerShellCommand(string arguments, bool readOutput, string processName = "cmd.exe")
        {
            try
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = false;
                if (readOutput) { startInfo.RedirectStandardOutput = true; } else { startInfo.RedirectStandardOutput = false; }
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = processName;
                startInfo.Arguments = "/c " + arguments;
                startInfo.CreateNoWindow = true;
                process.StartInfo = startInfo;
                process.Start();

                System.Threading.Thread.Sleep(10000);

                process.Close();
            }
            catch (Exception ex)
            {
                return "Error running CLI: " + ex.Message + " " + arguments;
            }

            return "";
        }

    }
}
