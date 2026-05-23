using Microsoft.EntityFrameworkCore;
using Clientes.Application.Abstractions;
using Clientes.Domain.Entities;
using Clientes.Infrastructure.Data;

namespace Clientes.Infrastructure.Repositories;

internal sealed class ClienteRepository(ClientesDbContext context) : IClienteRepository
{
    public async Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Clientes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.Clientes.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default) =>
        await context.Clientes.AddAsync(cliente, cancellationToken);

    public void Atualizar(Cliente cliente) =>
        context.Clientes.Update(cliente);

    public void Remover(Cliente cliente) =>
        context.Clientes.Remove(cliente);
}
