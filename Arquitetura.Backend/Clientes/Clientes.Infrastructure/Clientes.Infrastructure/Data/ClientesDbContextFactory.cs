using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clientes.Infrastructure.Data;

/// <summary>
/// Usado pelo dotnet-ef em tempo de design para criar migrations
/// sem precisar do host da aplicação.
/// </summary>
internal sealed class ClientesDbContextFactory : IDesignTimeDbContextFactory<ClientesDbContext>
{
    public ClientesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ClientesDbContext>()
            .UseSqlServer("Server=localhost;Database=ClientesDb;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new ClientesDbContext(options);
    }
}
