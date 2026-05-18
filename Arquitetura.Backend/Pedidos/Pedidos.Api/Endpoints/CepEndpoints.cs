using MediatR;
using Pedidos.Application.Features.Queries.ConsultarCep;

namespace Pedidos.Api.Endpoints;

/// <summary>
/// Endpoints para consulta de CEP via API externa ViaCEP.
/// Exposto no microserviço de Pedidos para uso no preenchimento de endereço de entrega.
/// </summary>
public static class CepEndpoints
{
    public static IEndpointRouteBuilder MapCepEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cep").WithTags("CEP");

        group.MapGet("/{cep}", ConsultarAsync)
             .WithName("ConsultarCep")
             .WithSummary("Consulta o endereço de um CEP")
             .WithDescription("Retorna os dados de endereço usando a API pública ViaCEP (https://viacep.com.br).");

        return app;
    }

    private static async Task<IResult> ConsultarAsync(string cep, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new ConsultarCepQuery(cep), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound(result.Error);
    }
}
