using Arquitetura.SharedKernel.Common;
using MediatR;
using Produtos.Application.Features.Queries.ObterProdutoPorId;

namespace Produtos.Application.Features.Queries.ListarProdutos;

public sealed record ListarProdutosQuery : IRequest<Result<IReadOnlyList<ProdutoResponse>>>;
