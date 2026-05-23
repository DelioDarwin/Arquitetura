using Arquitetura.SharedKernel.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clientes.Application.Features.Commands.CriarCliente;

public sealed record CriarClienteCommand(
    string Nome,
    string Email,
    string Cpf) : IRequest<Result<Guid>>;