using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using IoT.DeviceShadow.Grain.Contract;
using Newtonsoft.Json;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using IoT.DeviceShadow.Utils.Common;
using IoT.DeviceShadow.Utils.Messages;

namespace IoT.DeviceShadow.Client
{
    public class App
    {
        double queueConnectRetryMin = 1;

        double QueueConnectRetryMin
        {
            get { return queueConnectRetryMin * 1000 * 60; }
            set { queueConnectRetryMin = value; }
        }

        System.Timers.Timer myTimer = new System.Timers.Timer();


        private readonly AppSettings _settings;

        private IClusterClient client;

        public App(AppSettings settings)
        {
            _settings = settings;
        }

        public void Run()
        {

            CreateOrleansClient();

            BindWithRabbit();
            Console.WriteLine("Hello from App.cs");
            Console.ReadLine();
        }

        private void CreateOrleansClient()
        {
            try
            {
                var clientBuilder = new ClientBuilder()
                    //.UseLocalhostClustering()
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = _settings.OrleansClientSettingsList["DeviceShadow"].ClusterId;
                        options.ServiceId = _settings.OrleansClientSettingsList["DeviceShadow"].ServiceId;
                    })
                    .UseAdoNetClustering(options =>
                    {
                        options.Invariant = _settings.OrleansClientSettingsList["DeviceShadow"].ClusterInfo.Invariant;
                        options.ConnectionString = _settings.OrleansClientSettingsList["DeviceShadow"].ClusterInfo.ConnectionString;
                    })
                    .ConfigureLogging(logging => logging.AddConsole());

                client = clientBuilder.Build();
                client.Connect().Wait();

            }
            catch (Exception ex)
            {
                Thread.Sleep(3000);
            }
        }

        private void BindWithRabbit()
        {
            myTimer.Elapsed += bindQueue;

            QueueConnectRetryMin = 0.1;//  double.Parse(ConfigurationManager.AppSettings["QueueConnectRetryMin"], System.Globalization.CultureInfo.InvariantCulture);

            myTimer.Interval = QueueConnectRetryMin;
            myTimer.Start();
        }

        private void bindQueue(Object myObject, EventArgs myEventArgs)
        {
            myTimer.Enabled = false;

            string HostName = _settings.RabbitMQSubscriberSettings.RabbitMQHost;
            string UserName = _settings.RabbitMQSubscriberSettings.RabbitMQUser;
            string Password = _settings.RabbitMQSubscriberSettings.RabbitMQPassword;
            int Port = int.Parse(_settings.RabbitMQSubscriberSettings.RabbitPort);
            string VirtualHost = _settings.RabbitMQSubscriberSettings.VirtualHost;
            string QueueName = _settings.RabbitMQSubscriberSettings.RabbitMQQueue;

            int factoryTimeoutMin = 10;//int.Parse(ConfigurationManager.AppSettings["factoryTimeoutMin"]); // Amount of time protocol operations (e.g. queue.declare) are allowed to take
            int channelTimeoutMin = 10;//int.Parse(ConfigurationManager.AppSettings["channelTimeoutMin"]); // Amount of time protocol operations(e.g.queue.declare) are allowed to take

            try
            {
                var factory = new RabbitMQ.Client.ConnectionFactory
                {
                    HostName = HostName,
                    UserName = UserName,
                    Password = Password,
                    Port = Port,
                    VirtualHost = VirtualHost,
                };

                factory.ContinuationTimeout = new TimeSpan(0, factoryTimeoutMin, 0);

                var connection = factory.CreateConnection();

                connection.ConnectionShutdown += (a, b) =>
                {
                    myTimer.Enabled = true;
                };

                var channel = connection.CreateModel();
                channel.ContinuationTimeout = new TimeSpan(0, channelTimeoutMin, 0);

                var ok = channel.QueueDeclare(queue: QueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);

                consumer.ConsumerCancelled += (model, ea) =>
                {
                    myTimer.Enabled = true;
                };

                consumer.Registered += (model, ea) =>
                {
                };

                consumer.Shutdown += (model, ea) =>
                {
                    myTimer.Enabled = true;
                };

                consumer.Unregistered += (model, ea) =>
                {
                    myTimer.Enabled = true;
                };

                int counter = 1;
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        // Validate message
                        ValidatedMessage deviceMessage = Validation.ValidateProperties(message);

                        if (deviceMessage == null)
                        {
                            ((EventingBasicConsumer)model).Model.BasicAck(ea.DeliveryTag, false);
                            return;
                        }

                        var deviceGrain = client.GetGrain<ISensor>(deviceMessage.IMEI);

                        deviceGrain.ProcessMessage(deviceMessage);

                        ((EventingBasicConsumer)model).Model.BasicAck(ea.DeliveryTag, false);

                        counter++;
                    }
                    catch (Exception ex)
                    {
                        ((EventingBasicConsumer)model).Model.BasicAck(ea.DeliveryTag, false);
                        myTimer.Enabled = true;
                    }
                };

                channel.BasicConsume(queue: QueueName, consumer: consumer);
            }
            catch (Exception ex)
            {
                myTimer.Enabled = true;
            }

        }
    }
}
