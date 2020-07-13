using System;
using System.Collections.Generic;
using System.Text;

namespace IoT.DeviceShadow.Utils.Common
{
    public class AppSettings
    {
        public string ConnectionString { get; set; }
        public string LoadDataConnectionString { get; set; }
        public RabbitMQSubscriberSetting RabbitMQSubscriberSettings { get; set; }
        public Dictionary<string, OrleansClientSetting> OrleansClientSettingsList { get; set; }
        public RabbitMQPublisherSetting RabbitMQPublisherSettings { get; set; }
        public OrleansSiloSetting OrleansSiloSettings { get; set; }
        public VehicleConfiguration VehicleConfigurations { get; set; }
    }

    public class RabbitMQSubscriberSetting
    {
        public string RabbitMQHost { get; set; }
        public string RabbitMQUser { get; set; }
        public string RabbitMQPassword { get; set; }
        public string RabbitPort { get; set; }
        public string VirtualHost { get; set; }
        public string RabbitMQQueue { get; set; }
    }

    public class OrleansClientSetting
    {
        public string ClusterId { get; set; }
        public string ServiceId { get; set; }
        public ClusterInfo ClusterInfo { get; set; }
    }

    public class RabbitMQPublisherSetting
    {
        public string RabbitMQHost { get; set; }
        public string RabbitMQUser { get; set; }
        public string RabbitMQPassword { get; set; }
        public List<string> RabbitMQQueueList { get; set; }
        public List<string> RabbitMQExchangeList { get; set; }
        public string RabbitMQRoutingKey { get; set; }
    }

    public class OrleansSiloSetting
    {
        public ClusterInfo ClusterInfo { get; set; }
        public string ClusterId { get; set; }
        public string ServiceId { get; set; }
        public int AgeLimitInDays { get; set; }
        public int DashboardPort { get; set; }
    }

    public class ClusterInfo
    {
        public string ConnectionString { get; set; }
        public string Invariant { get; set; }
    }

    public class VehicleConfiguration
    {
        public int DeviceStatusSpeedLimit { get; set; }
        public int TemperatureMaxLimit { get; set; }
        public int TemperatureMinLimit { get; set; }
        public double IgnitionOnBatteryVolt { get; set; }
        public double IgnitionOffBatteryVolt { get; set; }
        public double IgnitionOnBatteryOverCharge { get; set; }
    }
}
