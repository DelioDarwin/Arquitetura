using Arquitetura.Application.Abstractions;
using Arquitetura.Application.Common;
using MediatR;

namespace Arquitetura.Application.Features.Produtos.Queries.ObterProdutoPorId;

internal sealed class ObterProdutoPorIdQueryHandler(
    IProdutoRepository repository) : IRequestHandler<ObterProdutoPorIdQuery, Result<ProdutoResponse>>
{
    public async Task<Result<ProdutoResponse>> Handle(ObterProdutoPorIdQuery request, CancellationToken cancellationToken)
    {
        var produto = await repository.ObterPorIdAsync(request.Id, cancellationToken);

        if (produto is null)
            return Result.Failure<ProdutoResponse>(Error.NaoEncontrado);

        var response = new ProdutoResponse(
            produto.Id,
            produto.Nome,
            produto.Descricao,
            produto.Preco,
            produto.Estoque,
            produto.CriadoEm,
            produto.AtualizadoEm);

        return Result.Success(response);
    }
}
