using MassTransit;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Orquestador.ProcesoPedido;
using Serilog;
using Serilog.Sinks.Elasticsearch;

var builder = Host.CreateApplicationBuilder(args);


const string serviceName = "Orquestador";
// Configurar Serilog

var elasticUri = Environment.GetEnvironmentVariable("ELASTIC_URI");
if (string.IsNullOrEmpty(elasticUri))
{
    throw new InvalidOperationException("La variable de entorno 'ELASTIC_URI' no está definida.");
}

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", serviceName)
    .WriteTo.Console()
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
    {
        IndexFormat = "applogs-{0:yyyy.MM}",
        AutoRegisterTemplate = true
    })
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);


var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL");

if (string.IsNullOrEmpty(redisUrl))
{
    throw new InvalidOperationException("La variable de entorno 'REDIS_URL' no está definida.");
}

// Configurar MassTransit con RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<PedidoMachineState, PedidoState>().RedisRepository(redisUrl);
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "amqp://guest:guest@host.docker.internal:5672";
        cfg.Host(rabbitHost);
        cfg.ConfigureEndpoints(context);
    });
});

// Configurar OpenTelemetry

var elasticApm = Environment.GetEnvironmentVariable("ELASTIC_APM_URI");
if (string.IsNullOrEmpty(elasticApm))
{
    throw new InvalidOperationException("La variable de entorno 'ELASTIC_APM_URI' no está definida.");
}

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName)
    .AddTelemetrySdk()
    .AddContainerDetector()
    .AddEnvironmentVariableDetector();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(resourceBuilder)
            .AddSource("MassTransit")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(elasticApm);
            });
    })
    .WithMetrics(metricProviderBuilder =>
    {
        metricProviderBuilder
            .SetResourceBuilder(resourceBuilder)
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddMeter("MassTransit")
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(elasticApm);
            });
    });



var host = builder.Build();
await host.RunAsync();
