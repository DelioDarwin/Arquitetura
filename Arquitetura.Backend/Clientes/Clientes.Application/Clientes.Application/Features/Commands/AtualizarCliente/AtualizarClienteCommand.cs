using Arquitetura.SharedKernel.Common;
using MediatR;

namespace Clientes.Application.Features.Commands.AtualizarCliente;

public sealed record AtualizarClienteCommand(
    Guid Id,
    string Nome,
    string Email,
    string Cpf) : IRequest<Result>;
