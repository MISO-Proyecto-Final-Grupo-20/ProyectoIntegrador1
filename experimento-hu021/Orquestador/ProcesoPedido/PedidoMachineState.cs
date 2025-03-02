using MassTransit;
using Mensajes;
using Mensajes.Inventarios;
using Mensajes.Logistica;
using Mensajes.Ventas;

namespace Orquestador.ProcesoPedido
{
    public class PedidoMachineState : MassTransitStateMachine<PedidoState>
    {
        public State ValidandoInventario { get; private set; }
        public State PreparandoEnvio { get; private set; }
        public Event<ProcesarPedido> IniciarProcesarPedido { get; private set; }
        public Event<ProductosDisponibles> ProductosDisponibles { get; private set; }
        public Event<PedidoListoParaEnvio> PedidoListoParaEnvio { get; private set; }


        public PedidoMachineState()
        {
            InstanceState(x => x.CurrentState);

            Event(() => IniciarProcesarPedido, x =>
            {
                x.CorrelateById(context => context.Message.Id);
                x.SelectId(context => context.Message.Id);
            });

            Event(() => ProductosDisponibles, x =>
            {
                x.CorrelateById(context => context.Message.IdProceso);
                x.SelectId(context => context.Message.IdProceso);
            });

            Event(() => PedidoListoParaEnvio, x =>
            {
                x.CorrelateById(context => context.Message.IdProceso);
                x.SelectId(context => context.Message.IdProceso);
            });




            Initially(
                    When(IniciarProcesarPedido)
                        .Then(AsignarValoresPedido)
                        .Then(contex => contex.Publish(new ValidarInventario(contex.Message.Id, contex.Message.Productos)))
                        .TransitionTo(ValidandoInventario)
                );

            During(ValidandoInventario,
            When(ProductosDisponibles).Then(context => context.Publish(new PrepararEnvioPedido(context.Message.IdProceso, context.Saga.Productos)))
            .TransitionTo(PreparandoEnvio)
            );

            During(PreparandoEnvio,
                When(PedidoListoParaEnvio).Then(context => context.Publish(new ConfirmarPedido(context.Saga.IdPedido)))
                    .Finalize()
                );


        }



        private static void AsignarValoresPedido(BehaviorContext<PedidoState, ProcesarPedido> context)
        {
            context.Saga.Productos = context.Message.Productos;
            context.Saga.IdPedido = context.Message.IdPedido;
        }
    }
}
