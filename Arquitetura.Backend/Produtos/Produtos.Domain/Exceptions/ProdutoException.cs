using Arquitetura.SharedKernel.Exceptions;

namespace Produtos.Domain.Exceptions;

public sealed class ProdutoException(string message) : DomainException(message);

public sealed class ProdutoNaoEncontradoException(Guid id)
    : DomainException($"Produto com Id '{id}' não foi encontrado.");
