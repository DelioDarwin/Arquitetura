using Pedidos.Application.Abstractions;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Pedidos.Infrastructure.Http;

/// <summary>
/// Implementação do cliente HTTP para a API externa ViaCEP (https://viacep.com.br).
/// Registrado via AddHttpClient para reuso de connections e gerenciamento de ciclo de vida.
/// </summary>
internal sealed class ViaCepClient(HttpClient httpClient) : IViaCepClient
{
    public async Task<ViaCepDto?> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"ws/{cep}/json/", cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken);

        // ViaCEP retorna { "erro": true } para CEPs inválidos, mesmo com status 200
        if (result is null || result.Erro == true)
            return null;

        return new ViaCepDto(
            result.Cep ?? string.Empty,
            result.Logradouro ?? string.Empty,
            result.Complemento ?? string.Empty,
            result.Bairro ?? string.Empty,
            result.Localidade ?? string.Empty,
            result.Uf ?? string.Empty,
            result.Ibge ?? string.Empty,
            result.Gia ?? string.Empty,
            result.Ddd ?? string.Empty,
            result.Siafi ?? string.Empty);
    }

    // Modelo interno de deserialização — não exposto fora desta classe
    private sealed class ViaCepResponse
    {
        [JsonPropertyName("cep")]
        public string? Cep { get; init; }

        [JsonPropertyName("logradouro")]
        public string? Logradouro { get; init; }

        [JsonPropertyName("complemento")]
        public string? Complemento { get; init; }

        [JsonPropertyName("bairro")]
        public string? Bairro { get; init; }

        [JsonPropertyName("localidade")]
        public string? Localidade { get; init; }

        [JsonPropertyName("uf")]
        public string? Uf { get; init; }

        [JsonPropertyName("ibge")]
        public string? Ibge { get; init; }

        [JsonPropertyName("gia")]
        public string? Gia { get; init; }

        [JsonPropertyName("ddd")]
        public string? Ddd { get; init; }

        [JsonPropertyName("siafi")]
        public string? Siafi { get; init; }

        [JsonPropertyName("erro")]
        public bool? Erro { get; init; }
    }
}
