using Google.Protobuf;

namespace WorkerOne
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults(additionalSources: new String[] { typeof(MessageBroker).Name });
            builder.Services.AddHostedService<Worker>();

            builder.AddRabbitMQClient(connectionName: "queue");

            builder.Services.AddSingleton<MessageBroker, MessageBroker>();

            var host = builder.Build();
            host.Run();
        }
    }
}