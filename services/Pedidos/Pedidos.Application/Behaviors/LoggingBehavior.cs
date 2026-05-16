using MediatR;
using Microsoft.Extensions.Logging;

namespace Pedidos.Application.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var name = typeof(TRequest).Name;
        logger.LogInformation("Iniciando {Request}", name);
        var response = await next(cancellationToken);
        logger.LogInformation("Concluído {Request}", name);
        return response;
    }
}
