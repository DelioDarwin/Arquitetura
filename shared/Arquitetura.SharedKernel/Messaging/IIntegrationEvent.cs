namespace Arquitetura.SharedKernel.Messaging;

/// <summary>
/// Contrato base para Integration Events — mensagens trocadas entre microserviços via broker.
/// Diferente dos Domain Events (intra-serviço), Integration Events cruzam fronteiras de serviço.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>Identificador único da mensagem para idempotência.</summary>
    Guid EventId { get; }

    /// <summary>Momento em que o evento foi gerado (UTC).</summary>
    DateTime OcorridoEm { get; }
}
