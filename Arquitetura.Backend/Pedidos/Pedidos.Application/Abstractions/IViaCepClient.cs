namespace Pedidos.Application.Abstractions;

/// <summary>
/// Contrato para consulta de endereço por CEP via API externa (ViaCEP).
/// Desacopla Pedidos.Application da implementação HTTP concreta.
/// </summary>
public interface IViaCepClient
{
    Task<ViaCepDto?> ConsultarCepAsync(string cep, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO que representa o retorno da API ViaCEP.
/// </summary>
public sealed record ViaCepDto(
    string Cep,
    string Logradouro,
    string Complemento,
    string Bairro,
    string Localidade,
    string Uf,
    string Ibge,
    string Gia,
    string Ddd,
    string Siafi);
