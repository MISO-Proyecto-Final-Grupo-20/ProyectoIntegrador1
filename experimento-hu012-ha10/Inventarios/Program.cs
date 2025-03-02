using Inventarios.Consumidores;
using MassTransit;

namespace Inventarios
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();

            var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            if (string.IsNullOrEmpty(rabbitHost))
            {
                throw new InvalidOperationException("La variable de entorno 'RABBITMQ_HOST' no está definida.");
            }

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumers((typeof(ConsumidorValidarInventario).Assembly));
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(rabbitHost);
                    cfg.ConfigureEndpoints(context);
                });
            });


            var app = builder.Build();
            app.UseAuthorization();

            app.Run();
        }
    }
}
