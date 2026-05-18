namespace Arquitetura.SharedKernel.Common;

/// <summary>
/// Representa um erro de negócio com código identificável e mensagem legível.
/// Compartilhado por todos os serviços.
/// </summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NaoEncontrado = new("Geral.NaoEncontrado", "O recurso solicitado não foi encontrado.");
    public static readonly Error Invalido = new("Geral.Invalido", "A requisição contém dados inválidos.");
    public static readonly Error Conflito = new("Geral.Conflito", "O recurso já existe.");
}
