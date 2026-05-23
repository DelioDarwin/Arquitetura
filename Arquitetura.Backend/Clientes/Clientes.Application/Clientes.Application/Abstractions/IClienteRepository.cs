using Clientes.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clientes.Application.Abstractions
{
    public interface IClienteRepository
    {
        Task<Cliente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken = default);
        Task AdicionarAsync(Cliente cliente, CancellationToken cancellationToken = default);
        void Atualizar(Cliente cliente);
        void Remover(Cliente cliente);
    }
}
