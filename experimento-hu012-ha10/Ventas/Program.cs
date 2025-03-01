using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Ventas.Constantes;
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

            var connectionString = Environment.GetEnvironmentVariable("connection_string");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La variable de entorno 'connection_string' no está definida.");
            }

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

            app.MapPost("/pedidos", async (PedidoRequest solicitudPedido, ApplicationDbContext contexto) =>
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
            });
            app.Run();
        }
    }
}
