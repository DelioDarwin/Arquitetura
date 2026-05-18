using Pedidos.Domain.Entities;

namespace Pedidos.Application.Abstractions;

public interface IPedidoRepository
{
    Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Pedido>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default);
    void Atualizar(Pedido pedido);
}
