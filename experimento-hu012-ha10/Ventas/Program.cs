using MassTransit;
using Mensajes;
using Mensajes.Comunes;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La variable de entorno 'CONNECTION_STRING' no está definida.");
            }

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
                });
            });



            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            // Add services to the container.
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //dbContext.Database.EnsureCreated();
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

            _ = app.MapPost("/pedidos", async (PedidoRequest solicitudPedido, ApplicationDbContext contexto, IPublishEndpoint _publishEndpoint) =>
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
