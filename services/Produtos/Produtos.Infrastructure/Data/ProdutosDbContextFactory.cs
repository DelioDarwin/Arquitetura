using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Produtos.Infrastructure.Data;

/// <summary>
/// Usado pelo dotnet-ef em tempo de design para criar migrations
/// sem precisar do host da aplicação.
/// </summary>
internal sealed class ProdutosDbContextFactory : IDesignTimeDbContextFactory<ProdutosDbContext>
{
    public ProdutosDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ProdutosDbContext>()
            .UseSqlServer("Server=localhost;Database=ProdutosDb;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new ProdutosDbContext(options);
    }
}
