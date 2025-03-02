using MassTransit;
using Mensajes.Comunes;
using Mensajes.Logistica;

namespace Logistica.Consumidores;

public class ConsumidorPreperarEnvioPedido : IConsumer<PrepararEnvioPedido>
{
    private readonly ILogger<ConsumidorPreperarEnvioPedido> _logger;

    public ConsumidorPreperarEnvioPedido(ILogger<ConsumidorPreperarEnvioPedido> logger)
    {
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<PrepararEnvioPedido> context)
    {
        ChaosEngine.SimularFalloAleatorio(10);
        _logger.LogInformation($"Preparando envio del pedido {context.Message.idProceso}");
        await context.Publish(new PedidoListoParaEnvio(context.Message.idProceso));
    }
}