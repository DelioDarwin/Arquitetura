namespace Arquitetura.Application.Abstractions;

/// <summary>
/// Abstração da unidade de trabalho para persistir mudanças de forma transacional.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
