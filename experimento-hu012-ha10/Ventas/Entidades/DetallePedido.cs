namespace Ventas.Entidades
{
    public class DetallePedido
    {
        public int Id { get; set; }
        public int IdPedido { get; set; } 
        public string Sku { get; set; }
        public int Cantidad { get; set; }
    }
}
