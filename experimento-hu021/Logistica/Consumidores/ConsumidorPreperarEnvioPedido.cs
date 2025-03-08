using MassTransit;
using Mensajes.Comunes;
using Mensajes.Logistica;
using Microsoft.Extensions.Options;

namespace Logistica.Consumidores;

public class ConsumidorPreperarEnvioPedido : IConsumer<PrepararEnvioPedido>
{
    private readonly ILogger<ConsumidorPreperarEnvioPedido> _logger;
    private readonly ConfiguracionPruebasCaos _configuracionPruebasCaos;

    public ConsumidorPreperarEnvioPedido(ILogger<ConsumidorPreperarEnvioPedido> logger, IOptions<ConfiguracionPruebasCaos> configuracionPruebasCaos)
    {
        _logger = logger;
        _configuracionPruebasCaos = configuracionPruebasCaos.Value;
    }
    public async Task Consume(ConsumeContext<PrepararEnvioPedido> context)
    {
        GeneradorFallas.SimularFalloAleatorio(_configuracionPruebasCaos.PorcentajeFallos);
        _logger.LogInformation($"Preparando envio del pedido {context.Message.idProceso}");
        await context.Publish(new PedidoListoParaEnvio(context.Message.idProceso));
    }
}