using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IoT.DeviceShadow.Grain;
using IoT.DeviceShadow.Utils.Publisher.Interfaces;
using IoT.DeviceShadow.Utils;
using IoT.DeviceShadow.Utils.Publisher;
using IoT.DeviceShadow.Utils.Common;

namespace IoT.DeviceShadow.Silo
{
    class Program
    {
        static readonly ManualResetEvent _siloStopped = new ManualResetEvent(false);

        static readonly object syncLock = new object();

        static ISiloHost host;

         static bool siloStopping = false;

        public static async Task Main(string[] args)
        {

            SetupApplicationShutdown();

            Console.Write("SiloPort: ");
            var siloPort =   int.Parse(Console.ReadLine());

            Console.Write("GatewayPort: ");
            var gatewayPort =  int.Parse(Console.ReadLine());

            var appSettings = LoadConfiguration();

            var siloBuilder = new SiloHostBuilder()
                .UseDashboard(options => { })
                .Configure<GrainCollectionOptions>(options =>
                {
                    options.CollectionAge = TimeSpan.FromDays(appSettings.OrleansSiloSettings.AgeLimitInDays);
                })
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = appSettings.OrleansSiloSettings.ClusterId;
                    options.ServiceId = appSettings.OrleansSiloSettings.ServiceId;
                })
                .AddMemoryGrainStorageAsDefault()
                .UseAdoNetClustering(options =>
                {
                    options.Invariant = appSettings.OrleansSiloSettings.ClusterInfo.Invariant;
                    options.ConnectionString = appSettings.OrleansSiloSettings.ClusterInfo.ConnectionString;
                })
                .ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(Sensor).Assembly).WithReferences())
                .Configure<ProcessExitHandlingOptions>(options => options.FastKillOnProcessExit = false)
                .ConfigureLogging(logging => logging.AddConsole());

            siloBuilder.ConfigureServices(svc => svc.AddSingleton<IPublisher<SensorState>, Publisher<SensorState>>());
            siloBuilder.ConfigureServices(svc => svc.AddSingleton(appSettings));
            using (host = siloBuilder.Build())
            {
                await host.StartAsync();
                 
                Console.ReadLine();
            }
            _siloStopped.WaitOne();
        }

        static void SetupApplicationShutdown()
        {
            /// Capture the user pressing Ctrl+C, to prevent the application from crashing ungracefully
            Console.CancelKeyPress += (s, a) =>
            {
                a.Cancel = true;
                
                lock (syncLock)
                {
                    if (!siloStopping)
                    {
                        siloStopping = true;
                        Task.Run(StopSilo).Ignore();
                    }
                }
            };
        }

        static async Task StopSilo()
        {
            await host.StopAsync();
            _siloStopped.Set();
        }

        public static AppSettings LoadConfiguration()
        {
            AppSettings appSettings = new AppSettings()
            {
                OrleansSiloSettings = new OrleansSiloSetting(),
                RabbitMQPublisherSettings = new RabbitMQPublisherSetting()
            };
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            ConfigurationBinder.Bind(builder.GetSection("AppSettings"), appSettings);
            
            appSettings.RabbitMQPublisherSettings.RabbitMQQueueList = ToList(builder.GetSection("AppSettings:RabbitMQPublisherSettings:RabbitMQQueueList").Value);
            appSettings.RabbitMQPublisherSettings.RabbitMQExchangeList = ToList(builder.GetSection("AppSettings:RabbitMQPublisherSettings:RabbitMQExchangeList").Value);
            return appSettings;
        }

        private static List<string> ToList(string key) 
        {
            var list = key.Split(',').ToList();

            if (string.IsNullOrEmpty(list.LastOrDefault()) || string.IsNullOrWhiteSpace(list.LastOrDefault()))
            {
                list.RemoveAt(list.Count - 1);
            }
            return list;
        }
    }
}
