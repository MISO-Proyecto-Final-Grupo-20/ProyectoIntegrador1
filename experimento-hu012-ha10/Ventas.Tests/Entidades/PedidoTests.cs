using Ventas.Constantes;
using Ventas.Entidades;
using Xunit;

namespace Ventas.Tests.Entidades
{
    public class PedidoTests
    {
        [Fact]
        public void Pedido_CrearNuevo_PropiedadesInicializadasCorrectamente()
        {
            // Arrange
            var idCliente = 123;
            var fecha = DateTime.UtcNow;
            
            // Act
            var pedido = new Pedido
            {
                IdCliente = idCliente,
                Fecha = fecha,
                Estado = EstadoPedido.RECIBIDO,
                DetallesPedido = new List<DetallePedido>()
            };
            
            // Assert
            Assert.Equal(idCliente, pedido.IdCliente);
            Assert.Equal(fecha, pedido.Fecha);
            Assert.Equal(EstadoPedido.RECIBIDO, pedido.Estado);
            Assert.Empty(pedido.DetallesPedido);
        }

        [Fact]
        public void DetallePedido_AgregarALista_SeAgregaCorrectamente()
        {
            // Arrange
            var pedido = new Pedido
            {
                IdCliente = 123,
                Fecha = DateTime.UtcNow,
                Estado = EstadoPedido.RECIBIDO,
                DetallesPedido = new List<DetallePedido>()
            };
            
            var detalle = new DetallePedido
            {
                Sku = "SKU001",
                Cantidad = 5
            };
            
            // Act
            pedido.DetallesPedido.Add(detalle);
            
            // Assert
            Assert.Single(pedido.DetallesPedido);
            Assert.Equal("SKU001", pedido.DetallesPedido[0].Sku);
            Assert.Equal(5, pedido.DetallesPedido[0].Cantidad);
        }
    }
}
