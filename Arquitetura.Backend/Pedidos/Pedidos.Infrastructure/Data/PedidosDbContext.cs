using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Abstractions;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Data;

internal sealed class PedidosDbContext(DbContextOptions<PedidosDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Pedido> Pedidos => Set<Pedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PedidosDbContext).Assembly);
}
