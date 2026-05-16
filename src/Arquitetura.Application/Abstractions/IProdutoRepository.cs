using Arquitetura.Domain.Entities;

namespace Arquitetura.Application.Abstractions;

public interface IProdutoRepository
{
    Task<Produto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(Produto produto, CancellationToken cancellationToken = default);
    void Atualizar(Produto produto);
    void Remover(Produto produto);
}
