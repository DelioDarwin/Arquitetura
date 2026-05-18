using Microsoft.EntityFrameworkCore;
using Produtos.Application.Abstractions;
using Produtos.Domain.Entities;
using Produtos.Infrastructure.Data;

namespace Produtos.Infrastructure.Repositories;

internal sealed class ProdutoRepository(ProdutosDbContext context) : IProdutoRepository
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
