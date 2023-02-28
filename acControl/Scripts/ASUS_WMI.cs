﻿using System.Management;
using System.Runtime.InteropServices;
using System;
using System.Linq;

//
// This is a optimised/simplified version of ASUSWmi.cs from https://github.com/seerge/g-helper
// I do not take credit for the full functionality of the code.
//

public class ASUSWmi
{
    const string FILE_NAME = @"\\.\\ATKACPI";
    const uint CONTROL_CODE = 0x0022240C;

    const uint DSTS = 0x53545344;
    const uint DEVS = 0x53564544;

    public const uint CPU_Fan = 0x00110013;
    public const uint GPU_Fan = 0x00110014;

    public const uint PerformanceMode = 0x00120075;

    public const uint GPUEco = 0x00090020;
    public const uint GPUMux = 0x00090016;
    public const uint eGPU = 0x00090019;

    public const uint BatteryLimit = 0x00120057;
    public const uint ScreenOverdrive = 0x00050019;

    public const uint DevsCPUFanCurve = 0x00110024;
    public const uint DevsGPUFanCurve = 0x00110025;

    public const int PerformanceBalanced = 0;
    public const int PerformanceTurbo = 1;
    public const int PerformanceSilent = 2;

    public const int GPUModeEco = 0;
    public const int GPUModeStandard = 1;
    public const int GPUModeUltimate = 2;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode, byte[] lpInBuffer, uint nInBufferSize, byte[] lpOutBuffer, uint nOutBufferSize, ref uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private const uint GENERIC_READ = 0x80000000;
    private const uint GENERIC_WRITE = 0x40000000;
    private const uint OPEN_EXISTING = 3;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    private const uint FILE_SHARE_READ = 1;
    private const uint FILE_SHARE_WRITE = 2;

    private IntPtr _handle;

    public ASUSWmi()
    {
        _handle = CreateFile(
            FILE_NAME,
            GENERIC_READ | GENERIC_WRITE,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            IntPtr.Zero,
            OPEN_EXISTING,
            FILE_ATTRIBUTE_NORMAL,
            IntPtr.Zero
        );
        if (_handle == new IntPtr(-1))
        {
            throw new Exception("Can't connect to ACPI");
        }
    }

    public void Control(uint dwIoControlCode, byte[] lpInBuffer, byte[] lpOutBuffer)
    {
        uint lpBytesReturned = 0;
        bool result = DeviceIoControl(
            _handle,
            dwIoControlCode,
            lpInBuffer,
            (uint)lpInBuffer.Length,
            lpOutBuffer,
            (uint)lpOutBuffer.Length,
            ref lpBytesReturned,
            IntPtr.Zero
        );
    }

    public void Close()
    {
        CloseHandle(_handle);
    }

    private byte[] CallMethod(uint methodId, byte[] args)
    {
        byte[] acpiBuf = new byte[8 + args.Length];
        byte[] outBuffer = new byte[20];

        BitConverter.GetBytes(methodId).CopyTo(acpiBuf, 0);
        BitConverter.GetBytes((uint)args.Length).CopyTo(acpiBuf, 4);
        Array.Copy(args, 0, acpiBuf, 8, args.Length);

        Control(CONTROL_CODE, acpiBuf, outBuffer);

        return outBuffer;
    }

    public void SetFanCurve(int device, byte[] curve)
    {
        if (device == 1)
            DeviceSet(DevsGPUFanCurve, curve);
        else
            DeviceSet(DevsCPUFanCurve, curve);
    }

    public byte[] GetFanCurve(int device, int mode = 0)
    {
        uint fan_mode;

        switch (mode)
        {
            case 1: fan_mode = 2; break;
            case 2: fan_mode = 1; break;
            default: fan_mode = 0; break;
        }

        if (device == 1)
            return DeviceGetBuffer(DevsGPUFanCurve, fan_mode);
        else
            return DeviceGetBuffer(DevsCPUFanCurve, fan_mode);
    }

    public void DeviceSet(uint deviceID, int status)
    {
        byte[] args = BitConverter.GetBytes(deviceID)
            .Concat(BitConverter.GetBytes(status))
            .ToArray();
        CallMethod(DEVS, args);
    }

    public void DeviceSet(uint deviceID, byte[] parameters)
    {
        byte[] args = BitConverter.GetBytes(deviceID)
            .Concat(parameters)
            .ToArray();
        CallMethod(DEVS, args);
    }

    public int DeviceGet(uint deviceID)
    {
        byte[] args = BitConverter.GetBytes(deviceID);
        byte[] status = CallMethod(DSTS, args);
        return BitConverter.ToInt32(status, 0) - 65536;
    }

    public int DeviceGet2(uint deviceID)
    {
        byte[] args = BitConverter.GetBytes(deviceID);
        byte[] status = CallMethod(DSTS, args);
        return BitConverter.ToInt32(status, 0);
    }

    public byte[] DeviceGetBuffer(uint deviceID, uint status = 0)
    {
        byte[] args = BitConverter.GetBytes(deviceID)
            .Concat(BitConverter.GetBytes(status))
            .ToArray();
        return CallMethod(DSTS, args);
    }

    public void SubscribeToEvents(Action<object, EventArrivedEventArgs> EventHandler)
    {
        ManagementEventWatcher watcher = new ManagementEventWatcher();
        watcher.EventArrived += new EventArrivedEventHandler(EventHandler);
        watcher.Scope = new ManagementScope("root\\wmi");
        watcher.Query = new WqlEventQuery("SELECT * FROM AsusAtkWmiEvent");
        watcher.Start();
    }
}
