using Arquitetura.SharedKernel.Common;
using MediatR;
using Clientes.Application.Abstractions;
using Clientes.Domain.Entities;
using Clientes.Application.Features.Commands.CriarCliente;

namespace Clientes.Application.Features.Commands.CriarCliente;

internal sealed class CriarClienteCommandHandler(
    IClienteRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CriarClienteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CriarClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = Cliente.Criar(request.Nome, request.Email, request.Cpf);
        await repository.AdicionarAsync(cliente, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(cliente.Id);
    }
}
