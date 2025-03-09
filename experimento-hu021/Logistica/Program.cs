using Logistica.Consumidores;
using MassTransit;
using Mensajes.Comunes;
using Mensajes.Logistica;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Logistica
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            //Configurar masstransit

            var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            if (string.IsNullOrEmpty(rabbitHost))
            {
                throw new InvalidOperationException("La variable de entorno 'RABBITMQ_HOST' no est� definida.");
            }

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumers((typeof(ConsumidorPreperarEnvioPedido).Assembly));
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost);
                    cfg.ConfigureEndpoints(context);
                    cfg.UseMessageRetry(r =>
                    {
                        r.Handle<ExcepcionServicio>();
                        r.Immediate(30);
                    });
                    cfg.UseInMemoryOutbox(context);
                });
            });

            // Confgurar serilog

            var elasticUri = Environment.GetEnvironmentVariable("ELASTIC_URI");
            if (string.IsNullOrEmpty(elasticUri))
            {
                throw new InvalidOperationException("La variable de entorno 'ELASTIC_URI' no est� definida.");
            }

            const string serviceName = "Logistica";
            builder.Host.UseSerilog((context, configuration) =>
            {
                var elasticUri = Environment.GetEnvironmentVariable("ELASTIC_URI") ??
                                 "http://elastic:fenix123@host.docker.internal:9200";
                configuration
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
                    {
                        IndexFormat = "applogs-{0:yyyy.MM}",
                        AutoRegisterTemplate = true
                    })
                    .Enrich.WithProperty("Application", serviceName);
            });

            // Configurar OpenTelemetry

            var elasticApm = Environment.GetEnvironmentVariable("ELASTIC_APM_URI");
            if (string.IsNullOrEmpty(elasticApm))
            {
                throw new InvalidOperationException("La variable de entorno 'ELASTIC_APM_URI' no est� definida.");
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
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
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
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddProcessInstrumentation()
                        .AddMeter("MassTransit")
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(elasticApm);
                        });
                });

            builder.Services.AddOptions<ConfiguracionPruebasCaos>()
                .Configure(options =>
                {
                    var porcentajeFalloEntorno = Environment.GetEnvironmentVariable("PORCENTAJE_FALLOS");

                    if (!string.IsNullOrEmpty(porcentajeFalloEntorno) &&
                        double.TryParse(porcentajeFalloEntorno, out var porcentajeFallo))
                    {
                        options.PorcentajeFallos = porcentajeFallo;
                    }
                    else
                    {
                        options.PorcentajeFallos = 0;
                    }
                });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.Run();
        }
    }
}
