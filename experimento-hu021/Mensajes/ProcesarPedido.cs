using Mensajes.Comunes;

namespace Mensajes
{
    public record ProcesarPedido(
        Guid Id,
        int IdPedido,
        List<ProductoPedido> Productos
    );

    

}
