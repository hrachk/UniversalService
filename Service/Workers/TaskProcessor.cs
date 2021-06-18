using Microsoft.Extensions.Logging;
using Service.Model;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Workers
{
    public class TaskProcessor
    {
        private readonly ILogger<TaskProcessor> logger;
        private readonly Settings settings;

        public TaskProcessor(ILogger<TaskProcessor> logger, Settings settings)
        {
            this.logger = logger;
            this.settings = settings;
        }

        public async Task RunAsync(int number, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            Func<int, int> fibbonachi = null;

            fibbonachi = (num) =>
            {
                if (num < 2) return 1;
                else return fibbonachi(num - 1) + fibbonachi(num - 2);
            };

            var result = await Task.Run(async () =>
            {
                await Task.Delay(1000);
                return Enumerable.Range(0, number).Select(n => fibbonachi(n));
            },token);

           using(var writer = new StreamWriter(settings.ResultPath,true,Encoding.UTF8))
            {
                writer.WriteLine(DateTime.Now.ToString() + " : " + string.Join(" ", result));
            }
            logger.LogInformation($"Task Finished.  Result: {string.Join(" ", result)}");   

        }
    }
}
