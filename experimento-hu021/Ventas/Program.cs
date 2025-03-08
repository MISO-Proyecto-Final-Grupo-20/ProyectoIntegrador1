using MassTransit;
using Mensajes;
using Mensajes.Comunes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Ventas.Constantes;
using Ventas.Consumidores;
using Ventas.Contextos;
using Ventas.Endpoints;
using Ventas.Entidades;

namespace Ventas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAllElasticApm(
                );

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La variable de entorno 'CONNECTION_STRING' no está definida.");
            }

            //Configurar masstransit

            var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            if (string.IsNullOrEmpty(rabbitHost))
            {
                throw new InvalidOperationException("La variable de entorno 'RABBITMQ_HOST' no está definida.");
            }
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumers((typeof(ConsumidorConfirmarPedido).Assembly));
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost);
                    cfg.ConfigureEndpoints(context);
                    cfg.UseMessageRetry(r =>
                    {
                        r.Handle<ExcepcionServicio>();
                        r.Immediate(10);
                    });
                    cfg.UseInMemoryOutbox(context);
                });
            });

            // Confgurar serilog

            var elasticUri = Environment.GetEnvironmentVariable("ELASTIC_URI");
            if (string.IsNullOrEmpty(elasticUri))
            {
                throw new InvalidOperationException("La variable de entorno 'ELASTIC_URI' no está definida.");
            }

            const string serviceName = "Ventas";
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
                throw new InvalidOperationException("La variable de entorno 'ELASTIC_APM_URI' no está definida.");
            }

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName)
                .AddTelemetrySdk()
                .AddContainerDetector()
                .AddEnvironmentVariableDetector();

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracerProviderBuilder =>
                {
                    tracerProviderBuilder
                        .AddSource("MassTransit")
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(elasticApm);
                        }
                        )
                        .AddConsoleExporter();
                })
                .WithMetrics(metricProviderBuilder =>
                {
                    metricProviderBuilder
                        .SetResourceBuilder(resourceBuilder)
                        .AddMeter("MassTransit")
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(elasticApm);
                        });
                });



            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // Add services to the container.
            builder.Services.AddAuthorization();

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

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();

            }


            app.MapGet("/pedidos/{id:int}", async Task<Results<NotFound,Ok<Pedido>>>(int id, ApplicationDbContext contexto) =>
            {
                var pedido = await contexto.Pedidos.Include(p => p.DetallesPedido).FirstOrDefaultAsync(p => p.Id == id);
                

                if (pedido is null)
                {
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(pedido);

            });

            app.MapPost("/pedidos", async (PedidoRequest solicitudPedido, ApplicationDbContext contexto, IPublishEndpoint _publishEndpoint) =>
            {
                var pedido = new Pedido
                {
                    IdCliente = solicitudPedido.ClienteId,
                    Estado = EstadoPedido.RECIBIDO,
                    Fecha = DateTime.UtcNow,
                    DetallesPedido = solicitudPedido.Productos.Select(p => new DetallePedido
                    {
                        Sku = p.Nombre,
                        Cantidad = p.Cantidad
                    }).ToList()
                };

                contexto.Pedidos.Add(pedido);

                await contexto.SaveChangesAsync();
                await ProcesarPedido(pedido, _publishEndpoint);

            });
            app.Run();
        }

        private  static async Task ProcesarPedido(Pedido pedido, IPublishEndpoint _publishEndpoint)
        {
            var mensajeProcesarPedido = new ProcesarPedido(Guid.NewGuid(),pedido.Id,pedido.DetallesPedido.Select( x => new ProductoPedido(x.Sku,x.Cantidad)).ToList());
            await _publishEndpoint.Publish(mensajeProcesarPedido);
        }
    }
}
