using Microsoft.EntityFrameworkCore;
using Clientes.Application.Abstractions;
using Clientes.Domain.Entities;

namespace Clientes.Infrastructure.Data;

public sealed class ClientesDbContext(DbContextOptions<ClientesDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClientesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
