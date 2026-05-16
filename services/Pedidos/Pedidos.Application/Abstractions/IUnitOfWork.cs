namespace Pedidos.Application.Abstractions;

/// <summary>
/// Abstrai o commit de transação sem expor o DbContext para a camada Application.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
