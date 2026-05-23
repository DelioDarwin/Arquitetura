using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Clientes.Infrastructure.Data;

namespace Clientes.Infrastructure;

public static class MigrationExtensions
{
    /// <summary>
    /// Aplica as migrations pendentes do ClientesDbContext.
    /// Chamado no startup da API para garantir que o banco esteja atualizado.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClientesDbContext>();
        await db.Database.MigrateAsync();
    }
}
