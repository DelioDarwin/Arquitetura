namespace Arquitetura.SharedKernel.Exceptions;

/// <summary>
/// Exceção base para violações de regras de negócio.
/// Cada serviço define suas subclasses específicas.
/// </summary>
public abstract class DomainException(string message) : Exception(message);
