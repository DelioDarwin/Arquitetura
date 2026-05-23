using Arquitetura.SharedKernel.Common;
using MediatR;
using Clientes.Application.Features.Queries.ObterClientesPorId;

namespace Clientes.Application.Features.Queries.ListarClientes;

public sealed record ListaClientesQuery : IRequest<Result<IReadOnlyList<ClienteResponse>>>;
