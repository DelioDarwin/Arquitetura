using Arquitetura.Application.Abstractions;
using Arquitetura.Domain.Entities;
using Arquitetura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Arquitetura.Infrastructure.Repositories;

internal sealed class ProdutoRepository(AppDbContext context) : IProdutoRepository
{
    public async Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Produtos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken = default) =>
        await context.Produtos.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AdicionarAsync(Produto produto, CancellationToken cancellationToken = default) =>
        await context.Produtos.AddAsync(produto, cancellationToken);

    public void Atualizar(Produto produto) =>
        context.Produtos.Update(produto);

    public void Remover(Produto produto) =>
        context.Produtos.Remove(produto);
}
