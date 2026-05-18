namespace Arquitetura.SharedKernel.Messaging;

/// <summary>
/// Publicado por Pedidos após um pedido ser confirmado.
/// Consumido por Produtos para descontar o estoque de cada item.
/// </summary>
public sealed record PedidoConfirmadoIntegrationEvent(
    Guid EventId,
    DateTime OcorridoEm,
    Guid PedidoId,
    Guid ClienteId,
    IReadOnlyList<ItemPedidoConfirmado> Itens) : IIntegrationEvent;

/// <summary>Representa um item do pedido confirmado.</summary>
public sealed record ItemPedidoConfirmado(
    Guid ProdutoId,
    int Quantidade);
