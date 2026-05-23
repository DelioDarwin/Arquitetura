using Arquitetura.SharedKernel.Common;
using MediatR;

namespace Clientes.Application.Features.Queries.ObterClientesPorId;

public sealed record ObterClientesPorIdQuery(Guid Id) : IRequest<Result<ClienteResponse>>;

public sealed record ClienteResponse(
    Guid Id,
    string Nome,
    string Email,
    string cpf,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);