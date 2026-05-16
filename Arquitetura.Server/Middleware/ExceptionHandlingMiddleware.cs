using Arquitetura.Domain.Exceptions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace Arquitetura.Server.Middleware;

/// <summary>
/// Captura exceções não tratadas e retorna respostas HTTP padronizadas (RFC 7807 Problem Details).
/// </summary>
internal sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Erro de validação: {Errors}", ex.Errors);
            await WriteResponseAsync(context, HttpStatusCode.UnprocessableEntity, "Erro de Validação",
                ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (DomainException ex)
        {
            logger.LogWarning("Regra de negócio violada: {Message}", ex.Message);
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, "Regra de Negócio", [ex.Message]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado");
            await WriteResponseAsync(context, HttpStatusCode.InternalServerError, "Erro Interno",
                ["Ocorreu um erro inesperado. Tente novamente mais tarde."]);
        }
    }

    private static Task WriteResponseAsync(
        HttpContext context,
        HttpStatusCode status,
        string title,
        IEnumerable<string> errors)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)status;

        var body = JsonSerializer.Serialize(new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title,
            status = (int)status,
            errors
        });

        return context.Response.WriteAsync(body);
    }
}
