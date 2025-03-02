using MassTransit;
using Mensajes.Comunes;

namespace Orquestador.ProcesoPedido
{
    public class PedidoState : SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public int Version { get; set; }
        public string CurrentState { get; set; }
        public List<ProductoPedido> Productos { get; set; }
        public int IdPedido { get; set; }
    }
}
