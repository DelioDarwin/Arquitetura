using Arquitetura.SharedKernel.Common;
using MediatR;
using Produtos.Application.Abstractions;

namespace Produtos.Application.Features.Commands.DeletarProduto;

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
