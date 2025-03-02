using Logistica.Consumidores;
using MassTransit;
using Mensajes.Logistica;

namespace Logistica
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            if (string.IsNullOrEmpty(rabbitHost))
            {
                throw new InvalidOperationException("La variable de entorno 'RABBITMQ_HOST' no está definida.");
            }

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumers((typeof(ConsumidorPreperarEnvioPedido).Assembly));
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost);
                    cfg.ConfigureEndpoints(context);
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.Run();
        }
    }
}
