using IoT.DeviceShadow.Utils.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace IoT.DeviceShadow.Utils
{
    [Serializable]
    public class SensorState
    {
        public Double latitude { get; set; }

        public Double longitude { get; set; }

        public string MsgId { get; set; }

        public string imei { get; set; }

        public DateTime timestamp { get; set; }

        public DateTime receivedTime { get; set; }

        public GPSStatus gpsStatus { get; set; } = GPSStatus.Unknown;

        public MainPowerStatus powerStatus { get; set; } = MainPowerStatus.Unknown;
    }
}