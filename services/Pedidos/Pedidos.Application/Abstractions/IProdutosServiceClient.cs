namespace Pedidos.Application.Abstractions;

/// <summary>
/// Contrato de comunicação com o serviço de Produtos via HTTP.
/// Desacopla Pedidos.Application da implementação HTTP real.
/// </summary>
public interface IProdutosServiceClient
{
    Task<ProdutoDto?> ObterProdutoAsync(Guid produtoId, CancellationToken cancellationToken = default);
}

public sealed record ProdutoDto(Guid Id, string Nome, decimal Preco, int Estoque);
