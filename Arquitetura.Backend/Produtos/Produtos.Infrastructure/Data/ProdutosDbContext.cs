using Microsoft.EntityFrameworkCore;
using Produtos.Application.Abstractions;
using Produtos.Domain.Entities;

namespace Produtos.Infrastructure.Data;

public sealed class ProdutosDbContext(DbContextOptions<ProdutosDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProdutosDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
