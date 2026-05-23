using Arquitetura.SharedKernel.Common;
using Clientes.Application.Abstractions;
using MediatR;

namespace Clientes.Application.Features.Commands.AtualizarCliente;

internal sealed class AtualizarClienteCommandHandler(
    IClienteRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<AtualizarClienteCommand, Result>
{
    public async Task<Result> Handle(AtualizarClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (cliente is null)
            return Result.Failure(new Error("Cliente.NaoEncontrado", "Cliente não encontrado."));

        cliente.Atualizar(request.Nome, request.Email, request.Cpf);
        repository.Atualizar(cliente);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
