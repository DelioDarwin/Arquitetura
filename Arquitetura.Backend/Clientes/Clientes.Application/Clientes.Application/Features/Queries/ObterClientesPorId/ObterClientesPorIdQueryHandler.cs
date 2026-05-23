using Arquitetura.SharedKernel.Common;
using Clientes.Application.Abstractions;
using MediatR;

namespace Clientes.Application.Features.Queries.ObterClientesPorId;

internal sealed class ObterClientesPorIdQueryHandler(
    IClienteRepository repository) : IRequestHandler<ObterClientesPorIdQuery, Result<ClienteResponse>>
{
    public async Task<Result<ClienteResponse>> Handle(ObterClientesPorIdQuery request, CancellationToken cancellationToken)
    {
        var cliente = await repository.ObterPorIdAsync(request.Id, cancellationToken);
        if (cliente is null)
            return Result.Failure<ClienteResponse>(Error.NaoEncontrado);

        return Result.Success(new ClienteResponse(
            cliente.Id, cliente.Nome, cliente.Email, cliente.Cpf,
            cliente.CriadoEm, cliente.AtualizadoEm));
    }
}
