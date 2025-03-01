namespace Ventas.Entidades;

public class Pedido
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public string Estado { get; set; }
    public DateTime Fecha { get; set; }
    public List<DetallePedido> DetallesPedido { get; set; }
}