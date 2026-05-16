using Arquitetura.Application.Common;
using Arquitetura.Application.Features.Produtos.Commands.AtualizarProduto;
using Arquitetura.Application.Features.Produtos.Commands.CriarProduto;
using Arquitetura.Application.Features.Produtos.Commands.DeletarProduto;
using Arquitetura.Application.Features.Produtos.Queries.ListarProdutos;
using Arquitetura.Application.Features.Produtos.Queries.ObterProdutoPorId;
using MediatR;

namespace Arquitetura.Server.Endpoints;

public sealed class ProdutosEndpoints : IEndpointGroup
{
    public void Map(RouteGroupBuilder group)
    {
        group.MapGet("/", ListarAsync)
            .WithName("ListarProdutos")
            .WithSummary("Lista todos os produtos");

        group.MapGet("/{id:guid}", ObterPorIdAsync)
            .WithName("ObterProdutoPorId")
            .WithSummary("Obtém um produto pelo Id");

        group.MapPost("/", CriarAsync)
            .WithName("CriarProduto")
            .WithSummary("Cria um novo produto");

        group.MapPut("/{id:guid}", AtualizarAsync)
            .WithName("AtualizarProduto")
            .WithSummary("Atualiza um produto existente");

        group.MapDelete("/{id:guid}", DeletarAsync)
            .WithName("DeletarProduto")
            .WithSummary("Remove um produto");
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
            ? Results.CreatedAtRoute("ObterProdutoPorId", new { id = result.Value }, result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> AtualizarAsync(Guid id, AtualizarProdutoRequest request, ISender sender, CancellationToken ct)
    {
        var command = new AtualizarProdutoCommand(id, request.Nome, request.Descricao, request.Preco, request.Estoque);
        var result = await sender.Send(command, ct);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> DeletarAsync(Guid id, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new DeletarProdutoCommand(id), ct);
        return result.IsSuccess ? Results.NoContent() : Results.NotFound();
    }
}

// Request DTO separado do Command pois o Id vem da rota
public sealed record AtualizarProdutoRequest(string Nome, string Descricao, decimal Preco, int Estoque);
