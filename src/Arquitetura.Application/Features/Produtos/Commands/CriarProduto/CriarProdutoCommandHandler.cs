using Arquitetura.Application.Abstractions;
using Arquitetura.Application.Common;
using Arquitetura.Domain.Entities;
using MediatR;

namespace Arquitetura.Application.Features.Produtos.Commands.CriarProduto;

internal sealed class CriarProdutoCommandHandler(
    IProdutoRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CriarProdutoCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CriarProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = Produto.Criar(request.Nome, request.Descricao, request.Preco, request.Estoque);

        await repository.AdicionarAsync(produto, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(produto.Id);
    }
}
