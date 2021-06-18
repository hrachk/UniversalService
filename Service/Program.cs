using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Service.Extensions.HostExtensions;
using Service.Model;
using Service.Services;
using Service.Services.TaskQueue;
using Service.Workers;
using System;
using System.Threading.Tasks;

namespace Service
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configBuilder =>
                {
                    configBuilder.AddJsonFile("config.json");
                    configBuilder.AddCommandLine(args);
                })
                .ConfigureLogging(configLoging =>
                {
                    configLoging.AddConsole();
                    configLoging.AddDebug();
                })
                .ConfigureServices((services)=> 
                {
                    services.AddHostedService<TaskSchedulerService>();
                    services.AddHostedService<WorkerService>();

                    services.AddSingleton<Settings>();
                    services.AddSingleton<TaskProcessor>();
                    services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
                   
                });



            await host.RunService();
        }

    }
}
