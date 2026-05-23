using Clientes.Application.Features.Commands.AtualizarCliente;
using Clientes.Application.Features.Commands.CriarCliente;
using Clientes.Application.Features.Commands.ExcluirCliente;
using Clientes.Application.Features.Queries.ListarClientes;
using Clientes.Application.Features.Queries.ObterClientesPorId;
using MediatR;

namespace Clientes.Api.Endpoints;

public static class ClientesEndpoints
{
    public static IEndpointRouteBuilder MapClientesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/clientes").WithTags("Clientes");

        group.MapGet("/", ListarAsync)
             .WithSummary("Lista todos os clientes");

        group.MapGet("/{id:guid}", ObterPorIdAsync)
             .WithSummary("Obtém um cliente pelo Id");

        group.MapPost("/", CriarAsync)
             .WithName("CriarCliente")
             .WithSummary("Cria um novo cliente");

        group.MapPut("/{id:guid}", AtualizarAsync)
             .WithSummary("Atualiza um cliente existente");

        group.MapDelete("/{id:guid}", ExcluirAsync)
             .WithSummary("Exclui um cliente");

        return app;
    }

    private static async Task<IResult> ListarAsync(
        ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ListaClientesQuery(), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ObterPorIdAsync(
        Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ObterClientesPorIdQuery(id), ct);
        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> CriarAsync(
        CriarClienteCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.CreatedAtRoute("CriarCliente", new { id = result.Value }, result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> AtualizarAsync(
        Guid id, AtualizarClienteRequest request, ISender sender, CancellationToken ct)
    {
        var command = new AtualizarClienteCommand(id, request.Nome, request.Email, request.Cpf);
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> ExcluirAsync(
        Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ExcluirClienteCommand(id), ct);
        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.Error);
    }
}

/// <summary>Request body para atualização de cliente.</summary>
internal sealed record AtualizarClienteRequest(string Nome, string Email, string Cpf);
