using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Produtos.Infrastructure.Data;

namespace Produtos.Infrastructure;

public static class MigrationExtensions
{
    /// <summary>
    /// Aplica as migrations pendentes do ProdutosDbContext.
    /// Chamado no startup da API para garantir que o banco esteja atualizado.
    /// </summary>
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProdutosDbContext>();
        await db.Database.MigrateAsync();
    }
}
