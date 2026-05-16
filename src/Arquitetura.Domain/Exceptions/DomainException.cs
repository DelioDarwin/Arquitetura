namespace Arquitetura.Domain.Exceptions;

/// <summary>
/// Exceção base para violações das regras de negócio do domínio.
/// Use subclasses específicas para cada tipo de violação.
/// </summary>
public abstract class DomainException(string message) : Exception(message);
