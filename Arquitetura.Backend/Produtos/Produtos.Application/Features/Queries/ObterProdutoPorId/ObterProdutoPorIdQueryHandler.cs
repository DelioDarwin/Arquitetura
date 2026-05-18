using Arquitetura.SharedKernel.Common;
using MediatR;
using Produtos.Application.Abstractions;

namespace Produtos.Application.Features.Queries.ObterProdutoPorId;

internal sealed class ObterProdutoPorIdQueryHandler(
    IProdutoRepository repository) : IRequestHandler<ObterProdutoPorIdQuery, Result<ProdutoResponse>>
{
    public async Task<Result<ProdutoResponse>> Handle(ObterProdutoPorIdQuery request, CancellationToken cancellationToken)
    {
        var produto = await repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (produto is null)
            return Result.Failure<ProdutoResponse>(Error.NaoEncontrado);

        return Result.Success(new ProdutoResponse(
            produto.Id, produto.Nome, produto.Descricao,
            produto.Preco, produto.Estoque, produto.CriadoEm, produto.AtualizadoEm));
    }
}
