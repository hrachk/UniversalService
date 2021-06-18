using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Service.Model;
using Microsoft.Extensions.Logging;
using Service.Workers;
using Service.Services.TaskQueue;

namespace Service.Services
{
    public class TaskSchedulerService : IHostedService, IDisposable
    {
        private Timer timer;
        private readonly IServiceProvider service;
        private readonly Settings _settings;
        private readonly ILogger logger;
        private readonly object SyncRoot = new object();

        private readonly Random random = new Random();

        public TaskSchedulerService(IServiceProvider service)
        {
            this.service = service;
            this._settings = service.GetRequiredService<Settings>();
            this.logger = service.GetRequiredService<ILogger<TaskSchedulerService>>();
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
           
            var interval = _settings?.RunInterval ?? 0;
            if(interval==0)
            {
                logger.LogWarning("checkinterval is not defined in settings.Set to default : 60 sec.");
                interval = 60;
            }
            timer = new Timer(
                (e) => ProcrssTask(), 
                null, 
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(interval));

            return Task.CompletedTask;
        }

        private void ProcrssTask()
        {
             if(Monitor.TryEnter(SyncRoot))
            {
                logger.LogInformation($"Process task started");


                for (int i = 0; i < 20; i++)
                {
                    DoWork();
                }
              

                logger.LogInformation($" Process task finished");

                Monitor.Exit(SyncRoot);
            }
             else
            {
                logger.LogInformation($" Processing is currently in progress. Skipped");
            }
        }

        void DoWork()
        {
            var number = random.Next(20);
            var process = service.GetRequiredService<TaskProcessor>();
            var queue = service.GetRequiredService<IBackgroundTaskQueue>();

            queue.QueueBackgroundWorkItem(token =>
            {

                return process.RunAsync(number, token);
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
