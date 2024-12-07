using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

namespace WorkerOne
{
    public sealed class MessageBroker : IDisposable
    {
        public static readonly String TraceActivityName = typeof(MessageBroker).FullName!;
        private static readonly ActivitySource TraceActivitySource = new(TraceActivityName);

        private readonly IConnection connection;
        private readonly IModel channel;

        public MessageBroker(
               IConfiguration configuration,
               IHostApplicationLifetime hostApplicationLifetime,IConnection connection)
        {
            this.connection = connection;
            hostApplicationLifetime.ApplicationStopping.ThrowIfCancellationRequested();

            channel = connection!.CreateModel();

            channel.QueueDeclare(
                queue: "test-messages",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            //var activityListener = new ActivityListener
            //{
            //    ShouldListenTo = s => true,
            //    SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
            //    Sample = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            //};
            //ActivitySource.AddActivityListener(activityListener);
        }

        public void PublishMessage(String message)
        {
            using var activity = TraceActivitySource.StartActivity("WorkerOne", ActivityKind.Producer);

            var basicProperties = channel.CreateBasicProperties();

            if (activity?.Id != null)
            {
                basicProperties.Headers = new Dictionary<string, object>
            {
                { "traceparent", activity.Id }
            };
            }

            channel.BasicPublish(
                exchange: string.Empty,
                routingKey: "test-messages",
                basicProperties: basicProperties,
                body: Encoding.UTF8.GetBytes(message));
        }

        public void Dispose()
        {
            channel.Close();
            connection.Close();
            channel.Dispose();
            connection.Dispose();
        }
    }
}