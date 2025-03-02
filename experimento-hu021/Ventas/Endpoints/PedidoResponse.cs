namespace Ventas.Endpoints
{

    record PedidoResponse(string PedidoId, string Estado, string Mensaje, decimal CostoTotal);
}
