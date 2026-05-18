namespace Arquitetura.SharedKernel.Primitives;

/// <summary>
/// Marcador de Domain Events. Implementado com MediatR.INotification
/// na camada Application de cada serviço para manter o Domain sem dependências.
/// </summary>
public interface IDomainEvent;
