using acControl.Models;
using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace acControl.Services
{
    public class XgMobileConnectionService
    {
        private readonly ASUSWmi wmi;
        private static readonly byte[] XG_MOBILE_CURVE_FUNC_NAME = { 0x5e, 0xd1, 0x01 };

        public bool Connected { get; private set; }
        public bool Detected { get; private set; }

        public class XgMobileStatusEvent
        {
            public bool Connected { get; init; }
            public bool Detected { get; init; }
            public bool DetectedChanged { get; init; }
            public bool ConnectedChanged { get; init; }
        }
        public event EventHandler<XgMobileStatusEvent>? XgMobileStatus;

        public XgMobileConnectionService(ASUSWmi wmi)
        {
            try
            {
                this.wmi = wmi;
                UpdateXgMobileStatus();
                wmi.SubscribeToEvents((a, b) => UpdateXgMobileStatus());
            } catch { return; }
        }

        private void UpdateXgMobileStatus()
        {
            try
            {
                bool prevDetected = Detected;
                bool prevConnected = Connected;
                Detected = IsEGPUDetected();
                if (Detected)
                {
                    Connected = IsEGPUConnected();
                }
                else
                {
                    Connected = false;
                }
                if (prevDetected != Detected || prevConnected != Connected)
                {
                    XgMobileStatus?.Invoke(this, new XgMobileStatusEvent
                    {
                        Detected = Detected,
                        Connected = Connected,
                        DetectedChanged = prevDetected != Detected,
                        ConnectedChanged = prevConnected != Connected
                    });
                }
            }
            catch { return; }
        }

        private bool IsEGPUDetected()
        {
            return wmi.DeviceGet(ASUSWmi.eGPUConnected) == 1;
        }

        private bool IsEGPUConnected()
        {
            int deviceStatus = wmi.DeviceGet(ASUSWmi.eGPU);
            if (deviceStatus != 0 && deviceStatus != 1)
            {
                throw new InvalidOperationException($"Unknown device status: {deviceStatus}");
            }
            return wmi.DeviceGet(ASUSWmi.eGPU) == 1;
        }

        public void EnableXgMobileLight()
        {
            SendXgMobileLightingCommand(new byte[] { 0x5e, 0xc5, 0x50 });
        }

        public void DisableXgMobileLight()
        {
            SendXgMobileLightingCommand(new byte[] { 0x5e, 0xc5 });
        }

        public bool SetXgMobileFan(List<CurvePoint> points)
        {
            if (!ValidatePoints(points))
            {
                return false;
            }
            var paramsBytes = new List<byte>(XG_MOBILE_CURVE_FUNC_NAME);
            paramsBytes.AddRange(points.Select(point => (byte)point.Fan)); // 8 bytes of fan
            paramsBytes.AddRange(points.Select(point => (byte)point.Temperature)); // 8 bytes of temperature
            return SendXgMobileLightingCommand(paramsBytes.ToArray());
        }

        private bool ValidatePoints(List<CurvePoint> points)
        {
            if (points.Count != 8)
            {
                return false;
            }
            return true;
        }

        private bool SendXgMobileLightingCommand(byte[] command)
        {
            var devices = HidDevices.Enumerate(0x0b05, new int[] { 0x1970 });
            var xgMobileLight = devices.Where(device => device.IsConnected && device.Description.ToLower().StartsWith("hid") && device.Capabilities.FeatureReportByteLength > 64).ToList();

            if (xgMobileLight.Count != 1)
            {
                return false;
            }
            var device = xgMobileLight[0];
            try
            {
                device.OpenDevice();
                var paramsArr = new byte[300];
                Array.Copy(command, paramsArr, command.Length);
                return device.WriteFeatureData(paramsArr);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                device.CloseDevice();
            }
        }
    }
}
