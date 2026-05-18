using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Infrastructure.Data;

namespace Pedidos.Infrastructure;

public static class MigrationExtensions
{
    /// <summary>
    /// Aplica as migrations pendentes do PedidosDbContext.
    /// Chamado no startup da API para garantir que o banco esteja atualizado.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
        await db.Database.MigrateAsync();
    }
}
