using Mensajes.Comunes;

namespace Mensajes.Logistica;

public record PrepararEnvioPedido(Guid idProceso, List<ProductoPedido> productos);