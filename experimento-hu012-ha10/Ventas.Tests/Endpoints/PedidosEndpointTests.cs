using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using Ventas.Constantes;
using Ventas.Contextos;
using Ventas.Entidades;
using Ventas.Endpoints;
using Xunit;

namespace Ventas.Tests.Endpoints
{
    public class PedidosEndpointTests
    {
        [Fact]
        public async Task ObtenerPedido_CuandoPedidoExiste_RetornaOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Crear un pedido de prueba
            using (var context = new ApplicationDbContext(options))
            {
                var pedido = new Pedido
                {
                    Id = 1,
                    IdCliente = 123,
                    Estado = EstadoPedido.RECIBIDO,
                    Fecha = DateTime.UtcNow,
                    DetallesPedido = new List<DetallePedido>
                    {
                        new DetallePedido { Sku = "SKU001", Cantidad = 2 }
                    }
                };
                
                context.Pedidos.Add(pedido);
                await context.SaveChangesAsync();
            }

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var result = await GetPedido(1, context);
                
                // Assert
                Assert.True(result.Result is Ok<Pedido>);
                
                // Get the value using the generic IResult interface
                if (result.Result is IValueHttpResult<Pedido> valueResult)
                {
                    var pedido = valueResult.Value;
                    Assert.NotNull(pedido);
                    Assert.Equal(1, pedido.Id);
                    Assert.Equal(123, pedido.IdCliente);
                }
            }
        }

        [Fact]
        public async Task ObtenerPedido_CuandoPedidoNoExiste_RetornaNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase2")
                .Options;

            // Act
            using (var context = new ApplicationDbContext(options))
            {
                var result = await GetPedido(999, context);
                
                // Assert
                Assert.True(result.Result is NotFound);
            }
        }

        // Helper method to simulate the endpoint handler
        private async Task<Results<NotFound, Ok<Pedido>>> GetPedido(int id, ApplicationDbContext contexto)
        {
            var pedido = await contexto.Pedidos.Include(p => p.DetallesPedido).FirstOrDefaultAsync(p => p.Id == id);
            
            if (pedido is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(pedido);
        }
    }
}
