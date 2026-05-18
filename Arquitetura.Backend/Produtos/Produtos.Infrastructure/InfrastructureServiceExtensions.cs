using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Produtos.Application.Abstractions;
using Produtos.Infrastructure.Data;
using Produtos.Infrastructure.Messaging;
using Produtos.Infrastructure.Repositories;

namespace Produtos.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ProdutosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ProdutosConnection")));

        services.AddScoped<IProdutoRepository, ProdutoRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ProdutosDbContext>());

        // Consumer de Integration Events via MassTransit / RabbitMQ
        services.AddMassTransit(x =>
        {
            x.AddConsumer<PedidoConfirmadoConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:Username"] ?? "guest");
                    h.Password(configuration["RabbitMQ:Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
