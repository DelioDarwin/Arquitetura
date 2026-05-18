using Arquitetura.SharedKernel.Common;
using MediatR;
using Pedidos.Application.Abstractions;
using Pedidos.Domain.Enums;

namespace Pedidos.Application.Features.Queries.ListarPedidosPorCliente;

public sealed record ListarPedidosPorClienteQuery(Guid ClienteId) : IRequest<Result<IReadOnlyList<PedidoResumoDto>>>;

public sealed record PedidoResumoDto(Guid Id, StatusPedido Status, decimal Total, DateTime CriadoEm);

internal sealed class ListarPedidosPorClienteQueryHandler(IPedidoRepository repository)
    : IRequestHandler<ListarPedidosPorClienteQuery, Result<IReadOnlyList<PedidoResumoDto>>>
{
    public async Task<Result<IReadOnlyList<PedidoResumoDto>>> Handle(
        ListarPedidosPorClienteQuery request, CancellationToken cancellationToken)
    {
        var pedidos = await repository.ListarPorClienteAsync(request.ClienteId, cancellationToken);
        var dtos = pedidos.Select(p => new PedidoResumoDto(p.Id, p.Status, p.Total, p.CriadoEm))
                          .ToList()
                          .AsReadOnly();
        return Result.Success<IReadOnlyList<PedidoResumoDto>>(dtos);
    }
}
