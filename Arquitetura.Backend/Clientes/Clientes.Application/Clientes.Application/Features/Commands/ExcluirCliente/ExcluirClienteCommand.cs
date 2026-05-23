using Arquitetura.SharedKernel.Common;
using MediatR;

namespace Clientes.Application.Features.Commands.ExcluirCliente;

public sealed record ExcluirClienteCommand(Guid Id) : IRequest<Result>;
