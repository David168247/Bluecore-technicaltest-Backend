using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using BluecoreApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace BluecoreApi.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // The client disconnected; do not attempt to write a response.
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (status, detail) = exception switch
            {
                DuplicateUserAccountException => (409, "El usuario o correo ya está registrado."),
                InvalidCredentialsException => (401, "Credenciales inválidas."),
                ValidationException => (400, exception.Message),
                ArgumentException => (400, "Los datos enviados no son válidos."),
                DbUpdateException or DbException => (503, "El servicio de datos no está disponible."),
                _ => (500, "Ocurrió un error interno en el servidor.")
            };
            if (status >= 500) logger.LogError(exception, "Request failed with HTTP {StatusCode}.", status);
            else logger.LogWarning("Request rejected with HTTP {StatusCode}.", status);
            context.Response.Clear();
            if (status == 401) context.Response.Headers.WWWAuthenticate = "Bearer";
            await WriteProblemAsync(context, status, detail);
        }
    }

    public static Task WriteProblemAsync(HttpContext context, int status, string detail)
    {
        context.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = Microsoft.AspNetCore.WebUtilities.ReasonPhrases.GetReasonPhrase(status),
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;
        return context.Response.WriteAsJsonAsync(problem, options: (System.Text.Json.JsonSerializerOptions?)null, contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }
}
