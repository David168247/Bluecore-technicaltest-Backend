using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using BluecoreApi.Data;
using BluecoreApi.DTOs;
using BluecoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
namespace BluecoreApi.Tests;

public sealed class AuthenticationApiTests
{
    private sealed class ApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=unused;Username=unused",
                    ["Jwt:Key"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                    ["Jwt:Issuer"] = "api-tests", ["Jwt:Audience"] = "api-tests"
                }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IUserAccountRepository>();
                services.AddSingleton<IUserAccountRepository>(new AuthenticationServiceTests.FakeRepository());
                services.RemoveAll<ICreditRequestService>();
                services.AddSingleton<ICreditRequestService>(new FakeCreditService());
            });
        }
    }

    private sealed class FakeCreditService : ICreditRequestService
    {
        public Task<BluecoreApi.Models.CreditRequest> CreateAsync(CreateCreditRequestDto dto, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<IEnumerable<BluecoreApi.Models.CreditRequest>> GetAllAsync(string? status, CancellationToken cancellationToken = default) =>
            Task.FromResult<IEnumerable<BluecoreApi.Models.CreditRequest>>([]);
        public Task<BluecoreApi.Models.CreditRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            Task.FromResult<BluecoreApi.Models.CreditRequest?>(null);
        public Task<BluecoreApi.Models.CreditRequest?> UpdateStatusAsync(int id, UpdateCreditStatusDto dto, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    [Fact]
    public async Task RegisterLoginAndAccessProtectedEndpoint()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var unauthenticated = await client.GetAsync("/api/credit-requests");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthenticated.StatusCode);
        Assert.Equal("application/problem+json", unauthenticated.Content.Headers.ContentType!.MediaType);
        var registration = new RegisterUserDto { Username = "tester", Email = "tester@example.com", Password = "long-test-password" };
        var registered = await client.PostAsJsonAsync("/api/auth/register", registration);
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        Assert.DoesNotContain("password", await registered.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/register", registration)).StatusCode);
        var failedLogin = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { UsernameOrEmail = "tester", Password = "wrong-password" });
        Assert.Equal(HttpStatusCode.Unauthorized, failedLogin.StatusCode);
        var login = await client.PostAsJsonAsync("/api/auth/login",
            new LoginDto { UsernameOrEmail = "tester", Password = registration.Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = await login.Content.ReadFromJsonAsync<AuthenticationResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/credit-requests")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/credit-requests/1")).StatusCode);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken + "tampered");
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/credit-requests")).StatusCode);
    }

    [Fact]
    public async Task InvalidRegistration_ReturnsValidationProblem()
    {
        await using var factory = new ApiFactory();
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterUserDto());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
    }
}
