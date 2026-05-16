using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Abstractions;
using Pedidos.Infrastructure.Data;
using Pedidos.Infrastructure.Http;
using Pedidos.Infrastructure.Messaging;
using Pedidos.Infrastructure.Repositories;

namespace Pedidos.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PedidosDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("PedidosConnection")));

        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<PedidosDbContext>());

        // Comunicação HTTP com o serviço de Produtos
        services.AddHttpClient<IProdutosServiceClient, ProdutosServiceClient>(client =>
            client.BaseAddress = new Uri(configuration["Services:ProdutosUrl"]
                ?? throw new InvalidOperationException("Services:ProdutosUrl não configurado.")));

        // Publisher de Integration Events via MassTransit / RabbitMQ
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddMassTransit(x =>
        {
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
