using HidLibrary;
using System;
using System.Diagnostics;
using System.Linq;

namespace acControl.Services
{
    public class XgMobileConnectionService
    {
        private readonly ASUSWmi wmi;

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
            this.wmi = wmi;
            UpdateXgMobileStatus();
            wmi.SubscribeToEvents((a, b) => UpdateXgMobileStatus());
        }

        private void UpdateXgMobileStatus()
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

        private void SendXgMobileLightingCommand(byte[] command)
        {
            var devices = HidDevices.Enumerate(0x0b05, new int[] { 0x1970 });
            var xgMobileLight = devices.Where(device => device.IsConnected && device.Description.ToLower().StartsWith("hid") && device.Capabilities.FeatureReportByteLength > 64).ToList();
            if (xgMobileLight.Count == 1)
            {
                var device = xgMobileLight[0];
                device.OpenDevice();
                var paramsArr = new byte[300];
                Array.Copy(command, paramsArr, command.Length);
                device.WriteFeatureData(paramsArr);
                device.CloseDevice();
            }
        }
    }
}
