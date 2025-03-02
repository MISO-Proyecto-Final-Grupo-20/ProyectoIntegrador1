using MassTransit;
using Mensajes.Comunes;
using Mensajes.Ventas;
using Ventas.Constantes;
using Ventas.Contextos;

namespace Ventas.Consumidores;

public class ConsumidorConfirmarPedido : IConsumer<ConfirmarPedido>
{
    private readonly ILogger<ConsumidorConfirmarPedido> _logger;
    private readonly ApplicationDbContext _dbContext;

    public ConsumidorConfirmarPedido(ILogger<ConsumidorConfirmarPedido> logger , ApplicationDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task Consume(ConsumeContext<ConfirmarPedido> context)
    {
        ChaosEngine.SimularFalloAleatorio(10);
        var pedido = await _dbContext.Pedidos.FindAsync(context.Message.IdPedido);
        pedido.Estado = EstadoPedido.CONFIRMADO;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"Pedido confirmado: {context.Message.IdPedido}");
    }
}