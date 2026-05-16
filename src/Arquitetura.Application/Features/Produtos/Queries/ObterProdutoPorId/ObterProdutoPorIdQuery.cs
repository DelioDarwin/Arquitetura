using Arquitetura.Application.Common;
using MediatR;

namespace Arquitetura.Application.Features.Produtos.Queries.ObterProdutoPorId;

public sealed record ObterProdutoPorIdQuery(Guid Id) : IRequest<Result<ProdutoResponse>>;

public sealed record ProdutoResponse(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);
