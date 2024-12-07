var builder = DistributedApplication.CreateBuilder(args);

var queue = builder.AddRabbitMQ(name: "queue")
    .WithLifetime(ContainerLifetime.Persistent);

var worker1 = builder.AddProject<Projects.WorkerOne>("workerone")
    .WithReference(queue)
    .WaitFor(queue);

var worker2 = builder.AddProject<Projects.WorkerTwo>("workertwo")
    .WithReference(queue)
    .WaitFor(queue)
    .WaitFor(worker1);

builder.Build().Run();
