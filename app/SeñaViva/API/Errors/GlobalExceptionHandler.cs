using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SenaViva.API.Errors;

public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            BadRequestException => (StatusCodes.Status400BadRequest, "Solicitud inválida", exception.Message),
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Error inesperado", "Ocurrió un error inesperado.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails { Status = statusCode, Title = title, Detail = detail }
        });
    }
}
