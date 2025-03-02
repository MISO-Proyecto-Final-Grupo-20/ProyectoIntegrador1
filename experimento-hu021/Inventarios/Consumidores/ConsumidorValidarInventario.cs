using MassTransit;
using Mensajes.Comunes;
using Mensajes.Inventarios;

namespace Inventarios.Consumidores;

public class ConsumidorValidarInventario : IConsumer<ValidarInventario>
{
    private readonly ILogger<ConsumidorValidarInventario> _logger;

    public ConsumidorValidarInventario(ILogger<ConsumidorValidarInventario> logger)
    {
        _logger = logger;
    }

    public  async Task Consume(ConsumeContext<ValidarInventario> context)
    {
        ChaosEngine.SimularFalloAleatorio(10);
        await context.Publish(new ProductosDisponibles(context.Message.IdProceso));
        _logger.LogInformation($"Inventario validado para la orden: {context.Message.IdProceso}");
    }
}