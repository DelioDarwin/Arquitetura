using Arquitetura.SharedKernel.Exceptions;

namespace Pedidos.Domain.Exceptions;

public sealed class PedidoException(string message) : DomainException(message);
