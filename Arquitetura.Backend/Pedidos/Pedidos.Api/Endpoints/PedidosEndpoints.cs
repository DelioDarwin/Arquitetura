using MediatR;
using Pedidos.Application.Features.Commands.CriarPedido;
using Pedidos.Application.Features.Queries.ListarPedidosPorCliente;

namespace Pedidos.Api.Endpoints;

public static class PedidosEndpoints
{
    public static IEndpointRouteBuilder MapPedidosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pedidos").WithTags("Pedidos");

        group.MapGet("/cliente/{clienteId:guid}", ListarPorClienteAsync)
             .WithSummary("Lista pedidos de um cliente");

        group.MapPost("/", CriarAsync)
             .WithName("CriarPedido")
             .WithSummary("Cria um novo pedido confirmado com os itens informados");

        return app;
    }

    private static async Task<IResult> ListarPorClienteAsync(
        Guid clienteId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ListarPedidosPorClienteQuery(clienteId), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CriarAsync(
        CriarPedidoCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.CreatedAtRoute("CriarPedido", new { id = result.Value }, result.Value)
            : Results.BadRequest(result.Error);
    }
}
