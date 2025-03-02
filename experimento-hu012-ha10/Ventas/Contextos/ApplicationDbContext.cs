using Microsoft.EntityFrameworkCore;
using Ventas.Entidades;

namespace Ventas.Contextos;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options): base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DetallePedido>()
            .HasOne<Pedido>()
            .WithMany(b => b.DetallesPedido)
            .HasForeignKey(p => p.IdPedido)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<DetallePedido> DetallesPedido { get; set; }
}