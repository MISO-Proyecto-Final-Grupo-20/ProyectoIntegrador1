using MassTransit;
using Orquestador.ProcesoPedido;

var builder = Host.CreateApplicationBuilder(args);


// Configurar MassTransit con RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<PedidoMachineState, PedidoState>().InMemoryRepository();
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "amqp://guest:guest@host.docker.internal:5672";
        cfg.Host(rabbitHost);
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
await host.RunAsync();
