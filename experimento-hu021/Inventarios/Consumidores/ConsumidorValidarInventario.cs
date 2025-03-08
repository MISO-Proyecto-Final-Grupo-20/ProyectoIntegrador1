using MassTransit;
using Mensajes.Comunes;
using Mensajes.Inventarios;
using Microsoft.Extensions.Options;

namespace Inventarios.Consumidores;

public class ConsumidorValidarInventario : IConsumer<ValidarInventario>
{
    private readonly ILogger<ConsumidorValidarInventario> _logger;
    private readonly ConfiguracionPruebasCaos _configuracionPruebasCaos;

    public ConsumidorValidarInventario(ILogger<ConsumidorValidarInventario> logger , IOptions<ConfiguracionPruebasCaos> configuracionCaos)
    {
        _logger = logger;
        _configuracionPruebasCaos = configuracionCaos.Value;
    }

    public  async Task Consume(ConsumeContext<ValidarInventario> context)
    {
        GeneradorFallas.SimularFalloAleatorio(_configuracionPruebasCaos.PorcentajeFallos);
        await context.Publish(new ProductosDisponibles(context.Message.IdProceso));
        _logger.LogInformation($"Inventario validado para la orden: {context.Message.IdProceso}");
    }
}