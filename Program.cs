using System.Threading.RateLimiting;
using BluecoreApi.Configuration;
using BluecoreApi.Data;
using BluecoreApi.Middleware;
using BluecoreApi.Services.Implementations;
using BluecoreApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString,
    postgres => postgres.MigrationsHistoryTable("__EFMigrationsHistory", "esquema_c")));
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddScoped<ICreditRequestService, CreditRequestService>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("authentication", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
        }));
});

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages(async statusContext =>
{
    var context = statusContext.HttpContext;
    var detail = context.Response.StatusCode switch
    {
        401 => "Se requiere un token de acceso válido.",
        403 => "No tiene permiso para acceder a este recurso.",
        404 => "Recurso no encontrado.",
        429 => "Demasiadas solicitudes. Intente nuevamente más tarde.",
        _ => "La solicitud no pudo completarse."
    };
    await ExceptionHandlingMiddleware.WriteProblemAsync(context, context.Response.StatusCode, detail);
});
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.Run();

public partial class Program;
