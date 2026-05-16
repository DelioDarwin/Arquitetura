using Arquitetura.Application.Abstractions;
using Arquitetura.Infrastructure.Data;
using Arquitetura.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Arquitetura.Infrastructure;

public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registra todos os serviços da camada Infrastructure: EF Core, Repositórios e UoW.
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}
