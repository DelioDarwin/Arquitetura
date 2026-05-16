using Arquitetura.SharedKernel.Messaging;
using MassTransit;
using Pedidos.Application.Abstractions;

namespace Pedidos.Infrastructure.Messaging;

/// <summary>
/// Implementação do IEventPublisher usando MassTransit como broker.
/// Injeta IPublishEndpoint que é fornecido pelo MassTransit DI.
/// </summary>
internal sealed class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent
        => publishEndpoint.Publish(integrationEvent, cancellationToken);
}
