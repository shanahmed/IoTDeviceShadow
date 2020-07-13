using IoT.DeviceShadow.Utils.Enum;
using System;

namespace IoT.DeviceShadow.Utils.Messages
{
    public class ValidatedMessage
    {
        public string IMEI { get; set; }

        public string MsgId { get; set; }

        public DateTime ReceivedTime { get; set; }
        
        public DateTime EventTime { get; set; }
        
        public double Latitude { get; set; }
        
        public double Longitude { get; set; }

        public GPSStatus GpsStatus { get; set; }

        public MainPowerStatus MainPowerStatus { get; set; }
    }
}
