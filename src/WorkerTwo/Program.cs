namespace WorkerTwo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.AddServiceDefaults();
            builder.Services.AddHostedService<Worker>();

            builder.AddRabbitMQClient(connectionName: "queue");

            var host = builder.Build();
            host.Run();
        }
    }
}