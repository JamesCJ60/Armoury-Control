using System;

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
    }
}
