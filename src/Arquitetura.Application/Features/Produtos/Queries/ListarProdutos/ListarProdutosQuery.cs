using Arquitetura.Application.Common;
using Arquitetura.Application.Features.Produtos.Queries.ObterProdutoPorId;
using MediatR;

namespace Arquitetura.Application.Features.Produtos.Queries.ListarProdutos;

public sealed record ListarProdutosQuery : IRequest<Result<IReadOnlyList<ProdutoResponse>>>;
