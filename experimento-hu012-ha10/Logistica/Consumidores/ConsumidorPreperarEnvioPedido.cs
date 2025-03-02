using MassTransit;
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
        _logger.LogInformation($"Preparando envio del pedido {context.Message.idProceso}");
        await context.Publish(new PedidoListoParaEnvio(context.Message.idProceso));
    }
}