using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Abstractions;
using Pedidos.Domain.Entities;
using Pedidos.Infrastructure.Data;

namespace Pedidos.Infrastructure.Repositories;

internal sealed class PedidoRepository(PedidosDbContext context) : IPedidoRepository
{
    public async Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Pedido>> ListarPorClienteAsync(Guid clienteId, CancellationToken cancellationToken = default) =>
        await context.Pedidos
            .AsNoTracking()
            .Include(p => p.Itens)
            .Where(p => p.ClienteId == clienteId)
            .ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default) =>
        await context.Pedidos.AddAsync(pedido, cancellationToken);

    public void Atualizar(Pedido pedido) =>
        context.Pedidos.Update(pedido);
}
