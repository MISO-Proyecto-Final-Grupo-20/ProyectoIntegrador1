using MassTransit;
using MassTransit.Configuration;
using Mensajes.Comunes;
using Mensajes.Ventas;
using Microsoft.Extensions.Options;
using Ventas.Constantes;
using Ventas.Contextos;

namespace Ventas.Consumidores;

public class ConsumidorConfirmarPedido : IConsumer<ConfirmarPedido>
{
    private readonly ILogger<ConsumidorConfirmarPedido> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly ConfiguracionPruebasCaos _configuracionPruebasCaos;

    public ConsumidorConfirmarPedido(ILogger<ConsumidorConfirmarPedido> logger , ApplicationDbContext dbContext, IOptions<ConfiguracionPruebasCaos> configuracionPruebasCaos)
    {
        _logger = logger;
        _dbContext = dbContext;
        _configuracionPruebasCaos = configuracionPruebasCaos.Value;
    }

    public async Task Consume(ConsumeContext<ConfirmarPedido> context)
    {
        GeneradorFallas.SimularFalloAleatorio(_configuracionPruebasCaos.PorcentajeFallos);
        var pedido = await _dbContext.Pedidos.FindAsync(context.Message.IdPedido);
        pedido.Estado = EstadoPedido.CONFIRMADO;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"Pedido confirmado: {context.Message.IdPedido}");
    }
}