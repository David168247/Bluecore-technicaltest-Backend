using BluecoreApi.Data;
using BluecoreApi.Services.Implementations;
using BluecoreApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string DatabaseSchema = "esquema_c";

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException( "ConnectionStrings:DefaultConnection is not configured.");
}

// PostgreSQL + Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable( "__EFMigrationsHistory", DatabaseSchema );
        });
});

// Controllers
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Dependency Injection
builder.Services.AddScoped<
    ICreditRequestService,
    CreditRequestService>();

var app = builder.Build();

// OpenAPI solamente en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();