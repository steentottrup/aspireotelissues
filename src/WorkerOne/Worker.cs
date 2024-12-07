using Microsoft.AspNetCore.Components.Forms;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace WorkerOne
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private MessageBroker broker;

        public Worker(ILogger<Worker> logger, MessageBroker broker)
        {
            this.logger = logger;
            this.broker = broker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    this.broker.PublishMessage("Hello world!");

                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(60 * 1000, stoppingToken);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
