using Arquitetura.Application.Abstractions;
using Arquitetura.Application.Common;
using MediatR;

namespace Arquitetura.Application.Features.Produtos.Commands.DeletarProduto;

internal sealed class DeletarProdutoCommandHandler(
    IProdutoRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeletarProdutoCommand, Result>
{
    public async Task<Result> Handle(DeletarProdutoCommand request, CancellationToken cancellationToken)
    {
        var produto = await repository.ObterPorIdAsync(request.Id, cancellationToken);

        if (produto is null)
            return Result.Failure(Error.NaoEncontrado);

        repository.Remover(produto);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
