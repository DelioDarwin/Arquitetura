using MediatR;
using Microsoft.Extensions.Logging;

namespace Arquitetura.Application.Behaviors;

/// <summary>
/// Pipeline do MediatR que loga entrada, saída e duração de cada request.
/// </summary>
internal sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Processando {RequestName}", requestName);

        var response = await next(cancellationToken);

        logger.LogInformation("Concluído {RequestName}", requestName);

        return response;
    }
}
