using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.WindowsService
{
    class ServiceBaseLifeTime : ServiceBase, IHostLifetime
    {
        private readonly TaskCompletionSource<object> _delayStart = new TaskCompletionSource<object>();

         
        public ServiceBaseLifeTime(IApplicationLifetime applicationLifetime)
        {
            ApplicationLifetime = applicationLifetime ?? throw new ArgumentException(nameof(applicationLifetime));
        }

        
        private IApplicationLifetime ApplicationLifetime { get; }

       
        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => _delayStart.TrySetCanceled());
            ApplicationLifetime.ApplicationStopping.Register(Stop);
            new Thread(Run).Start();
            return _delayStart.Task;
        }

        private void Run(object obj)
        {
            try
            {
                Run(this);
                _delayStart.TrySetException(new InvalidOperationException("Stopped without starting"));
            }
            catch (Exception ex)
            {

                _delayStart.TrySetException(ex);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }

        protected override void OnStart(string[] args)
        {
            _delayStart.TrySetResult(null);
            base.OnStart(args);
        }

        
        protected override void OnStop()
        {
            ApplicationLifetime.StopApplication();
            base.OnStop();
        }

    }
}
