using Arquitetura.SharedKernel.Common;
using MediatR;
using Pedidos.Application.Abstractions;

namespace Pedidos.Application.Features.Queries.ConsultarCep;

/// <summary>
/// Query para consultar o endereço de um CEP via API externa ViaCEP.
/// </summary>
public sealed record ConsultarCepQuery(string Cep) : IRequest<Result<ViaCepDto>>;

internal sealed class ConsultarCepQueryHandler(
    IViaCepClient viaCepClient) : IRequestHandler<ConsultarCepQuery, Result<ViaCepDto>>
{
    public async Task<Result<ViaCepDto>> Handle(ConsultarCepQuery request, CancellationToken cancellationToken)
    {
        var endereco = await viaCepClient.ConsultarCepAsync(request.Cep, cancellationToken);

        if (endereco is null)
            return Result.Failure<ViaCepDto>(new Error("Cep.NaoEncontrado", $"CEP '{request.Cep}' não encontrado."));

        return Result.Success(endereco);
    }
}
