using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using Orleans;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using IoT.DeviceShadow.Grain.Contract;
using IoT.DeviceShadow.Utils;
using IoT.DeviceShadow.Utils.Publisher.Interfaces;
using IoT.DeviceShadow.Utils.Messages;
using IoT.DeviceShadow.Utils.Enum;

namespace IoT.DeviceShadow.Grain
{
    public class Sensor : Orleans.Grain<SensorState>, ISensor
    {
        private readonly IPublisher<SensorState> _publisher;

        IConfiguration config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

        private readonly ILogger logger;

        public Sensor(ILogger<Sensor> _logger, IPublisher<SensorState> publisher)
        {
            this.logger = _logger;
            this._publisher = publisher;
        }

        public Task SetGrainState(SensorState state)
        {
            this.State = state;
            return Task.CompletedTask;
        }

        public Task<SensorState> GetGrainState()
        {
            return Task.FromResult(this.State);
        }

        public Task ProcessMessage(ValidatedMessage message)
        {
            try
            {
                logger.LogInformation("Start Processing");
                logger.LogInformation("Device IMEI:" + message.IMEI);

                if (message.GpsStatus == GPSStatus.Valid)
                {
                    if (this.State.powerStatus == MainPowerStatus.Off && message.MainPowerStatus == MainPowerStatus.Off)
                    {
                        return Task.CompletedTask;
                    }

                    this.State.imei = message.IMEI;
                    this.State.gpsStatus = message.GpsStatus;
                    this.State.timestamp = message.EventTime;
                    this.State.receivedTime = message.ReceivedTime;


                    this.State.latitude = message.Latitude;
                    this.State.longitude = message.Longitude;
                    this.State.MsgId = message.MsgId;
                    this.State.powerStatus = message.MainPowerStatus;
                }
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                //this.GetLogger().Info("{0},{1},{2},{3}", message.EventTime, "Error:" + ex.ToString(), message.DeviceImei, message.EventName);

                return Task.CompletedTask;
            }
        }

        private void PubishActorState()
        {
            _publisher.PublishSync(this.State);
        }

        public override async Task OnActivateAsync()
        {
            if (_publisher == null)
            {
                await _publisher.Start();
            }

            await base.OnActivateAsync();
        }

        public override async Task OnDeactivateAsync()
        {
            if (_publisher != null)
            {
                await _publisher.Stop();
            }
            await base.OnDeactivateAsync();
        }
    }
}