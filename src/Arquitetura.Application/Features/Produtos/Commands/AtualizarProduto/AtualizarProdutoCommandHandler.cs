using Arquitetura.Application.Abstractions;
using Arquitetura.Application.Common;
using Arquitetura.Domain.Exceptions;
using MediatR;

namespace Arquitetura.Application.Features.Produtos.Commands.AtualizarProduto;

internal sealed class AtualizarProdutoCommandHandler(
    IProdutoRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<AtualizarProdutoCommand, Result>
{
    public async Task<Result> Handle(AtualizarProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await repository.ObterPorIdAsync(request.Id, cancellationToken);

        if (produto is null)
            return Result.Failure(Error.NaoEncontrado);

        produto.Atualizar(request.Nome, request.Descricao, request.Preco, request.Estoque);
        repository.Atualizar(produto);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
