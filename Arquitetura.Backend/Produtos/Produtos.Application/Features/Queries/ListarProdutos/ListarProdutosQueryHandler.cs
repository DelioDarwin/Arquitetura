using Arquitetura.SharedKernel.Common;
using MediatR;
using Produtos.Application.Abstractions;
using Produtos.Application.Features.Queries.ObterProdutoPorId;

namespace Produtos.Application.Features.Queries.ListarProdutos;

internal sealed class ListarProdutosQueryHandler(
    IProdutoRepository repository) : IRequestHandler<ListarProdutosQuery, Result<IReadOnlyList<ProdutoResponse>>>
{
    public async Task<Result<IReadOnlyList<ProdutoResponse>>> Handle(ListarProdutosQuery request, CancellationToken cancellationToken)
    {
        var produtos = await repository.ListarAsync(cancellationToken);

        var response = produtos
            .Select(p => new ProdutoResponse(p.Id, p.Nome, p.Descricao, p.Preco, p.Estoque, p.CriadoEm, p.AtualizadoEm))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<ProdutoResponse>>(response);
    }
}
