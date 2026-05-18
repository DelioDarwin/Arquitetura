using MediatR;
using Produtos.Application.Features.Commands.AtualizarProduto;
using Produtos.Application.Features.Commands.CriarProduto;
using Produtos.Application.Features.Commands.DeletarProduto;
using Produtos.Application.Features.Queries.ListarProdutos;
using Produtos.Application.Features.Queries.ObterProdutoPorId;

namespace Produtos.Api.Endpoints;

public static class ProdutosEndpoints
{
    public static IEndpointRouteBuilder MapProdutosEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/produtos").WithTags("Produtos");

        group.MapGet("/", ListarAsync).WithSummary("Lista todos os produtos");
        group.MapGet("/{id:guid}", ObterPorIdAsync).WithName("ObterProduto").WithSummary("Obtém produto por Id");
        group.MapPost("/", CriarAsync).WithSummary("Cria um novo produto");
        group.MapPut("/{id:guid}", AtualizarAsync).WithSummary("Atualiza um produto");
        group.MapDelete("/{id:guid}", DeletarAsync).WithSummary("Remove um produto");

        return app;
    }

    private static async Task<IResult> ListarAsync(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ListarProdutosQuery(), ct);
        return Results.Ok(result.Value);
    }

    private static async Task<IResult> ObterPorIdAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ObterProdutoPorIdQuery(id), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
    }

    private static async Task<IResult> CriarAsync(CriarProdutoCommand command, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);
        return result.IsSuccess
            ? Results.CreatedAtRoute("ObterProduto", new { id = result.Value }, result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> AtualizarAsync(Guid id, AtualizarProdutoRequest request, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new AtualizarProdutoCommand(id, request.Nome, request.Descricao, request.Preco, request.Estoque), ct);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> DeletarAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeletarProdutoCommand(id), ct);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }
}

public sealed record AtualizarProdutoRequest(string Nome, string Descricao, decimal Preco, int Estoque);
