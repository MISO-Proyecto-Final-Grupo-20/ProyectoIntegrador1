namespace Ventas.Endpoints
{
    record PedidoRequest(
        int ClienteId,
        List<Producto> Productos
    );

    record Producto(string Nombre, int Cantidad);


}
