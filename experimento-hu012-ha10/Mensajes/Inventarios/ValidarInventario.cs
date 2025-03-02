using Mensajes.Comunes;

namespace Mensajes.Inventarios
{
    public record ValidarInventario(
        Guid IdProceso ,
        List<ProductoPedido> Productos );
}
