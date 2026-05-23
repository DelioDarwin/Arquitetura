using Arquitetura.SharedKernel.Common;
using Clientes.Application.Abstractions;
using MediatR;

namespace Clientes.Application.Features.Commands.ExcluirCliente;

internal sealed class ExcluirClienteCommandHandler(
    IClienteRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<ExcluirClienteCommand, Result>
{
    public async Task<Result> Handle(ExcluirClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (cliente is null)
            return Result.Failure(new Error("Cliente.NaoEncontrado", "Cliente não encontrado."));

        repository.Remover(cliente);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
