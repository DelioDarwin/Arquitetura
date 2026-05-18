using Arquitetura.SharedKernel.Messaging;

namespace Pedidos.Application.Abstractions;

/// <summary>
/// Abstrai a publicação de Integration Events para desacoplar
/// a camada Application do broker concreto (RabbitMQ, Azure Service Bus, etc.).
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent;
}
