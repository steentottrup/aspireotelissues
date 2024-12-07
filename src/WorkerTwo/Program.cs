namespace WorkerTwo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults(additionalSources: new String[] { typeof(Worker).Name });
            builder.Services.AddHostedService<Worker>();

            builder.AddRabbitMQClient(connectionName: "queue");

            var host = builder.Build();
            host.Run();
        }
    }
}