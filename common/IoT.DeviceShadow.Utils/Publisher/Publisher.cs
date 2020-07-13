using IoT.DeviceShadow.Utils.Common;
using IoT.DeviceShadow.Utils.Publisher.Interfaces;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IoT.DeviceShadow.Utils.Publisher
{
    public class Publisher<T> :IPublisher<T> where T : class
    {
        private IBusControl _bus { get; set; }
        private BusHandle _busHandle { get; set; }

        private readonly AppSettings _settings;

        public Publisher(AppSettings settings) 
        {
            _settings = settings;

            
            _bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(String.Format("rabbitmq://{0}/", _settings.RabbitMQPublisherSettings.RabbitMQHost)), h =>
                {
                    if (!String.IsNullOrWhiteSpace(_settings.RabbitMQPublisherSettings.RabbitMQUser))
                    {
                        h.Username(_settings.RabbitMQPublisherSettings.RabbitMQUser);
                    }
                    if (!String.IsNullOrWhiteSpace(_settings.RabbitMQPublisherSettings.RabbitMQPassword))
                    {
                        h.Password(_settings.RabbitMQPublisherSettings.RabbitMQPassword);

                    }

                    foreach (var queue in _settings.RabbitMQPublisherSettings.RabbitMQQueueList)
                    {
                        foreach (var exchange in _settings.RabbitMQPublisherSettings.RabbitMQExchangeList)
                        {
                            cfg.Publish<T>(c => c.BindQueue(exchange, queue, z => z.RoutingKey = _settings.RabbitMQPublisherSettings.RabbitMQRoutingKey));
                        }
                    }
                });
            });
        }

        public async Task Start()
        {
            _busHandle = await _bus.StartAsync();
        }

        public async Task Stop()
        {
            await _bus.StopAsync();
        }

        public Task PublishSync(object message)
        {
            return Task.FromResult(_bus.Publish(message));
        }
    }
}
