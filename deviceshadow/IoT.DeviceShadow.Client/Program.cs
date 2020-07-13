using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using IoT.DeviceShadow.Utils.Common;

namespace IoT.DeviceShadow.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = ConfigureServices();

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetService<App>().Run();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            var settings = LoadConfiguration();
            services.AddSingleton(settings);

            // Run the application
            services.AddTransient<App>();

            return services;
        }
          
        public static AppSettings LoadConfiguration()
        {
            AppSettings appSettings = new AppSettings()
            {
                OrleansClientSettingsList = new Dictionary<string, OrleansClientSetting>(),
                RabbitMQSubscriberSettings = new RabbitMQSubscriberSetting()
            };
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

            ConfigurationBinder.Bind(builder.GetSection("AppSettings"), appSettings);

            return appSettings;
        }
    }
}