using System.Text.Json;
using BluecoreApi.Exceptions;
using BluecoreApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
namespace BluecoreApi.Tests;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Theory]
    [InlineData("duplicate", 409)]
    [InlineData("credentials", 401)]
    [InlineData("database", 503)]
    [InlineData("unexpected", 500)]
    public async Task ErrorResponse_IsConsistentAndDoesNotExposeInternalDetails(string kind, int status)
    {
        Exception exception = kind switch
        {
            "duplicate" => new DuplicateUserAccountException(),
            "credentials" => new InvalidCredentialsException(),
            "database" => new Microsoft.EntityFrameworkCore.DbUpdateException("internal-secret"),
            _ => new InvalidOperationException("internal-secret")
        };
        var middleware = new ExceptionHandlingMiddleware(_ => throw exception, NullLogger<ExceptionHandlingMiddleware>.Instance);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        await middleware.InvokeAsync(context);
        context.Response.Body.Position = 0;
        var json = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Equal(status, context.Response.StatusCode);
        Assert.StartsWith("application/problem+json", context.Response.ContentType);
        Assert.DoesNotContain("internal-secret", json);
        Assert.Equal(status, JsonDocument.Parse(json).RootElement.GetProperty("status").GetInt32());
    }
}
