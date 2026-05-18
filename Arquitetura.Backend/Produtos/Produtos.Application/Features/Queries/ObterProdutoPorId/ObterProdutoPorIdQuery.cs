using Arquitetura.SharedKernel.Common;
using MediatR;

namespace Produtos.Application.Features.Queries.ObterProdutoPorId;

public sealed record ObterProdutoPorIdQuery(Guid Id) : IRequest<Result<ProdutoResponse>>;

public sealed record ProdutoResponse(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);
