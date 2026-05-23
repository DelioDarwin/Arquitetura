using Arquitetura.SharedKernel.Common;
using MediatR;
using Clientes.Application.Abstractions;
using Clientes.Application.Features.Queries.ObterClientesPorId;

namespace Clientes.Application.Features.Queries.ListarClientes;

internal sealed class ListaClientesQueryHandler(
    IClienteRepository repository) : IRequestHandler<ListaClientesQuery, Result<IReadOnlyList<ClienteResponse>>>
{
    public async Task<Result<IReadOnlyList<ClienteResponse>>> Handle(ListaClientesQuery request, CancellationToken cancellationToken)
    {
        var clientes = await repository.ListarAsync(cancellationToken);
        var response = clientes
            .Select(c => new ClienteResponse(c.Id, c.Nome, c.Email, c.Cpf, c.CriadoEm, c.AtualizadoEm))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<ClienteResponse>>(response);
    }
}
