using Arquitetura.SharedKernel.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clientes.Domain.Exceptions
{

    // herda de DomainException do SharedKernel, igual ao ProdutoException
    public sealed class ClienteException(string message) : DomainException(message);
}
