using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Clientes.Application.Abstractions;
using Clientes.Infrastructure.Data;
using Clientes.Infrastructure.Repositories;

namespace Clientes.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ClientesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ClientesConnection")));

        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClientesDbContext>());

        return services;
    }

}
