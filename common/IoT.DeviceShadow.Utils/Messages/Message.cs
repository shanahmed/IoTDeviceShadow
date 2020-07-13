namespace IoT.DeviceShadow.Utils.Messages
{
    public class Message
    {
        public string IMEI { get; set; }

        public int MsgId { get; set; }

        public long ReceievedTime { get; set; }

        public DeviceState State { get; set; }
    }

    public class DeviceState
    {
        public long Time { get; set; }

        public string Latitude { get; set; }
        
        public string Longitude { get; set; }
        
        public string GpsStatus { get; set; }

        public string PowerStatus { get; set; }
    }
}
