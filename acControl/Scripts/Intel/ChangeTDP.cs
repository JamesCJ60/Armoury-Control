﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace acControl.Scripts.Intel
{
    public class ChangeTDP
    {
        static string cpuType = "";
        static string MCHBAR = "";
        static string RWDelay = "800";
        private static Object objLock = new Object();

        public static string BaseDir = App.location;


        //Change TDP routines - Intel
        public static void changeTDP(int pl1TDP, int pl2TDP)
        {
            //Return Success as default value, otherwise alert calling routine to error
            try
            {
                determineCPU();

                if (cpuType == "Intel")
                {
                    //if (Properties.Settings.Default.IntelMMIOMSR.Contains("MMIO")){runIntelTDPChangeMMIO(pl1TDP, pl2TDP);}
                    /*if (Properties.Settings.Default.IntelMMIOMSR == "MSRCMD") {}*/
                    runIntelTDPChangeMSRCMD(pl1TDP, pl2TDP);
                    //else { runIntelTDPChangeMSR(pl1TDP, pl2TDP); }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs:  Changing TDP: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);

            }

        }

        static void runIntelTDPChangeMMIO(int pl1TDP, int pl2TDP)
        {
            try
            {
                string processRW = BaseDir + "\\Assets\\Intel\\RW\\Rw.exe";
                string hexPL1 = convertTDPToHexMMIO(pl1TDP);
                string hexPL2 = convertTDPToHexMMIO(pl2TDP);
                if (hexPL1 != "Error" && hexPL2 != "Error" && MCHBAR != null)
                {
                    lock (objLock)
                    {
                        string commandArguments = " /nologo /stdout /command=" + '\u0022' + "Delay " + RWDelay + "; w16 " + MCHBAR + "a0 0x" + hexPL1 + "; Delay " + RWDelay + "; w16 " + MCHBAR + "a4 0x" + hexPL2 + "; Delay " + RWDelay + ";" + '\u0022';

                        RunCLI.RunCommand(commandArguments, false, processRW);
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs:  Run Intel TDP Change: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);

            }


        }

        //MMIO Stuff here
        static string convertTDPToHexMMIO(int tdp)
        {
            //Convert integer TDP value to Hex for rw.exe
            //Must use formula (TDP in watt   *1000/125) +32768 and convert to hex
            try
            {
                int newTDP = (tdp * 1000 / 125) + 32768;
                return newTDP.ToString("X");

            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs:  convert MMIO TDP To Hex: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);
                return "Error";
            }
        }

        static void determineCPU()
        {
            try
            {
                if (cpuType != "Intel" && cpuType != "AMD")
                {
                    //Get the processor name to determine intel vs AMD
                    object processorNameRegistry = Registry.GetValue("HKEY_LOCAL_MACHINE\\hardware\\description\\system\\centralprocessor\\0", "ProcessorNameString", null);
                    string processorName = null;
                    if (processorNameRegistry != null)
                    {
                        //If not null, find intel or AMD string and clarify type. For Intel determine MCHBAR for rw.exe
                        processorName = processorNameRegistry.ToString();
                        if (processorName.IndexOf("Intel") >= 0) { cpuType = "Intel"; }
                    }
                }
                if (cpuType == "Intel" && MCHBAR == "")
                {
                    determineIntelMCHBAR();
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs: Determining CPU type: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);
            }
        }

        static void determineIntelMCHBAR()
        {
            try
            {
                //Get the processor model to determine MCHBAR, INTEL ONLY
                object processorModelRegistry = Registry.GetValue("HKEY_LOCAL_MACHINE\\hardware\\description\\system\\centralprocessor\\0", "Identifier", null);
                string processorModel = null;
                if (processorModelRegistry != null)
                {
                    //If not null, convert to string and determine MCHBAR for rw.exe
                    processorModel = processorModelRegistry.ToString();
                    if (processorModel.IndexOf("Model 140") >= 0) { MCHBAR = "0xFEDC59"; } else { MCHBAR = "0xFED159"; };
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs: Determining MCHBAR: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);
            }

        }


        static string convertTDPToHex(int tdp)
        {
            //Convert integer TDP value to Hex for rw.exe
            //Must use formula (TDP in watt   *1000/125) +32768 and convert to hex
            try
            {
                int newTDP = (tdp * 1000 / 125) + 32768;
                return newTDP.ToString("X");

            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs:  convert TDP To Hex: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);
                return "Error";
            }
        }

        static void runIntelTDPChangeMSR(int pl1TDP, int pl2TDP)
        {
            try
            {
                string processRW = BaseDir + "\\Assets\\Intel\\RW\\Rw.exe";
                string hexPL1 = convertTDPToHexMSR(pl1TDP);
                string hexPL2 = convertTDPToHexMSR(pl2TDP);
                if (hexPL1 != "Error" && hexPL2 != "Error" && MCHBAR != null)
                {
                    if (hexPL1.Length < 3)
                    {
                        if (hexPL1.Length == 1) { hexPL1 = "00" + hexPL1; }
                        if (hexPL1.Length == 2) { hexPL1 = "0" + hexPL1; }
                    }
                    if (hexPL2.Length < 3)
                    {
                        if (hexPL2.Length == 1) { hexPL2 = "00" + hexPL2; }
                        if (hexPL2.Length == 2) { hexPL2 = "0" + hexPL2; }
                    }
                    lock (objLock)
                    {
                        string commandArguments = " /nologo /stdout /command=" + '\u0022' + "wrmsr 0x610 0x00438" + hexPL2 + " 0x00dd8" + hexPL1 + ";" + '\u0022';

                        RunCLI.RunCommand(commandArguments, false, processRW);
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs:  Run Intel TDP Change: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);

            }


        }

        static string runIntelTDPChange(int pl1TDP, int pl2TDP)
        {
            try
            {
                string processRW = BaseDir + "\\Assets\\Intel\\RW\\Rw.exe";
                string hexPL1 = convertTDPToHex(pl1TDP);
                string hexPL2 = convertTDPToHex(pl2TDP);
                if (hexPL1 != "Error" && hexPL2 != "Error" && MCHBAR != null)
                {
                    lock (objLock)
                    {
                        string commandArguments = " /nologo /stdout /command=" + '\u0022' + "Delay " + RWDelay + "; w16 " + MCHBAR + "a0 0x" + hexPL1 + "; Delay " + RWDelay + "; w16 " + MCHBAR + "a4 0x" + hexPL2 + "; Delay " + RWDelay + ";" + '\u0022';

                        RunCLI.RunCommand(commandArguments, false, processRW);
                        Thread.Sleep(100);
                        return "Success";

                    }

                }

                else { return "Error"; }


            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs:  Run Intel TDP Change: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);
                return "Error";
            }


        }

        static void runIntelTDPChangeMSRCMD(int pl1TDP, int pl2TDP)
        {
            try
            {
                string processRW = "cmd.exe";
                string hexPL1 = convertTDPToHexMSR(pl1TDP);
                string hexPL2 = convertTDPToHexMSR(pl2TDP);
                if (hexPL1 != "Error" && hexPL2 != "Error" && MCHBAR != null)
                {
                    lock (objLock)
                    {
                        if (hexPL1.Length < 3)
                        {
                            if (hexPL1.Length == 1) { hexPL1 = "00" + hexPL1; }
                            if (hexPL1.Length == 2) { hexPL1 = "0" + hexPL1; }
                        }
                        if (hexPL2.Length < 3)
                        {
                            if (hexPL2.Length == 1) { hexPL2 = "00" + hexPL2; }
                            if (hexPL2.Length == 2) { hexPL2 = "0" + hexPL2; }
                        }
                        string commandArguments = BaseDir + "\\Assets\\Intel\\MSR\\msr-cmd.exe -s write 0x610 0x00438" + hexPL2 + " 0x00dd8" + hexPL1;

                        RunCLI.RunCommand(commandArguments, false, processRW);
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs:  Run Intel TDP Change: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);

            }


        }

        static string convertTDPToHexMSR(int tdp)
        {
            //Convert integer TDP value to Hex for rw.exe
            //Must use formula (TDP in watt   *1000/125) +32768 and convert to hex
            try
            {
                int newTDP = (tdp * 8);
                return newTDP.ToString("X");

            }
            catch (Exception ex)
            {
                string errorMsg = "Error: ChangeTDP.cs:  convert MSR TDP To Hex: " + ex.Message;
                StreamWriterLog.startStreamWriter(errorMsg);
                MessageBox.Show(errorMsg);
                return "Error";
            }
        }

    }
}
