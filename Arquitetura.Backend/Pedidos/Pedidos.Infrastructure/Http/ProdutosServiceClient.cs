using Pedidos.Application.Abstractions;
using System.Net.Http.Json;

namespace Pedidos.Infrastructure.Http;

/// <summary>
/// Comunicação síncrona via HTTP com o microserviço de Produtos.
/// Registrado via AddHttpClient para reuso de connections.
/// </summary>
internal sealed class ProdutosServiceClient(HttpClient httpClient) : IProdutosServiceClient
{
    public async Task<ProdutoDto?> ObterProdutoAsync(Guid produtoId, CancellationToken cancellationToken = default) =>
        await httpClient.GetFromJsonAsync<ProdutoDto>($"api/produtos/{produtoId}", cancellationToken);
}
