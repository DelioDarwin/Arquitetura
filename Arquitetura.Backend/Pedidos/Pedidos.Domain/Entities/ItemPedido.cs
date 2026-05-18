using Arquitetura.SharedKernel.Primitives;

namespace Pedidos.Domain.Entities;

/// <summary>
/// Item de pedido como value object com identidade (owned entity no EF Core).
/// </summary>
public sealed record ItemPedido(
    Guid Id,
    Guid PedidoId,
    Guid ProdutoId,
    string NomeProduto,
    decimal PrecoUnitario,
    int Quantidade);
