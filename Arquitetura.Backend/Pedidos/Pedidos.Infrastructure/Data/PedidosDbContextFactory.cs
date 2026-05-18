using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Pedidos.Infrastructure.Data;

/// <summary>
/// Usado pelo dotnet-ef em tempo de design para criar migrations
/// sem precisar do host da aplicação.
/// </summary>
internal sealed class PedidosDbContextFactory : IDesignTimeDbContextFactory<PedidosDbContext>
{
    public PedidosDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PedidosDbContext>()
            .UseSqlServer("Server=localhost;Database=PedidosDb;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new PedidosDbContext(options);
    }
}
