using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace WorkerTwo
{
    public class Worker : BackgroundService
    {
        public static readonly string TraceActivityName = typeof(Worker).FullName!;
        private static readonly ActivitySource TraceActivitySource = new(TraceActivityName);

        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly ILogger<Worker> logger;

        public Worker(ILogger<Worker> logger, IConnection connection)
        {
            this.logger = logger;
            this.connection = connection;
            channel = connection!.CreateModel();

            channel.QueueDeclare(
                queue: "test-messages",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            // The following was added to get an activity from the StartActivity method (and not just null)
            var activityListener = new ActivityListener
            {
                ShouldListenTo = s => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            };
            ActivitySource.AddActivityListener(activityListener);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {

                    var consumer = new EventingBasicConsumer(this.channel);
                    consumer.Received += (_, e) => ProcessMessage(e);

                    channel.BasicConsume(
                        queue: "test-messages",
                        autoAck: true,
                        consumer: consumer);


                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void ProcessMessage(BasicDeliverEventArgs e)
        {
            string? parentActivityId = null;
            if (e.BasicProperties?.Headers?.TryGetValue("traceparent", out var parentActivityIdRaw) == true &&
                parentActivityIdRaw is byte[] traceParentBytes)
                parentActivityId = Encoding.UTF8.GetString(traceParentBytes);

            using var activity = TraceActivitySource.StartActivity(nameof(ProcessMessage), kind: ActivityKind.Consumer, parentId: parentActivityId);

            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);


            activity?.Stop();
            activity?.SetEndTime(DateTime.UtcNow);

            logger.LogInformation("Received message: {Message}", message);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            channel.Close();
            connection.Close();
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            base.Dispose();


            channel.Dispose();
            connection.Dispose();
        }
    }
}
