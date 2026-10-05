using BluecoreApi.DTOs;
using BluecoreApi.Exceptions;
using BluecoreApi.Models;
using BluecoreApi.Services.Implementations;
using BluecoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Xunit;
namespace BluecoreApi.Tests;

public sealed class AuthenticationServiceTests
{
    internal sealed class FakeRepository : IUserAccountRepository
    {
        public List<UserAccount> Accounts { get; } = [];
        public bool SimulateDuplicate { get; set; }
        public Task<bool> ExistsAsync(string username, string email, CancellationToken cancellationToken) =>
            Task.FromResult(Accounts.Any(x => x.NormalizedUsername == username || x.NormalizedEmail == email));
        public Task<UserAccount?> FindByIdentifierAsync(string identifier, CancellationToken cancellationToken) =>
            Task.FromResult(Accounts.SingleOrDefault(x => x.NormalizedUsername == identifier || x.NormalizedEmail == identifier));
        public Task UpdatePasswordHashAsync(Guid id, string passwordHash, CancellationToken cancellationToken)
        {
            Accounts.Single(x => x.Id == id).PasswordHash = passwordHash;
            return Task.CompletedTask;
        }
        public Task AddAsync(UserAccount account, CancellationToken cancellationToken)
        {
            if (SimulateDuplicate) throw new DuplicateUserAccountException();
            Accounts.Add(account);
            return Task.CompletedTask;
        }
    }

    internal static ITokenService TestTokenService() => new JwtTokenService(
        Microsoft.Extensions.Options.Options.Create(new BluecoreApi.Configuration.JwtOptions
        {
            Key = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)),
            Issuer = "tests",
            Audience = "tests"
        }), TimeProvider.System);

    [Theory]
    [InlineData("DAVID")]
    [InlineData("david@example.com")]
    public async Task Login_AcceptsUsernameOrEmail(string identifier)
    {
        var repository = new FakeRepository();
        var service = new AuthenticationService(repository, new PasswordHasher<UserAccount>(), TestTokenService());
        await service.RegisterAsync(ValidRegistration());
        var response = await service.LoginAsync(new() { UsernameOrEmail = identifier, Password = "long-test-password" });
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(repository.Accounts[0].Id, response.User.Id);
        Assert.NotEmpty(response.AccessToken);
    }

    [Theory]
    [InlineData("David", "wrong-password")]
    [InlineData("unknown", "long-test-password")]
    public async Task Login_UsesSameFailureForUnknownUserAndWrongPassword(string identifier, string password)
    {
        var repository = new FakeRepository();
        var service = new AuthenticationService(repository, new PasswordHasher<UserAccount>(), TestTokenService());
        await service.RegisterAsync(ValidRegistration());
        var exception = await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            service.LoginAsync(new() { UsernameOrEmail = identifier, Password = password }));
        Assert.Equal("Credenciales inválidas.", exception.Message);
    }

    private static RegisterUserDto ValidRegistration() =>
        new() { Username = "David", Email = "David@example.com", Password = "long-test-password" };

    [Fact]
    public async Task Register_PersistsHashAndNormalizedIdentifiers()
    {
        var repository = new FakeRepository();
        var hasher = new PasswordHasher<UserAccount>();
        var service = new AuthenticationService(repository, hasher, TestTokenService());
        var dto = ValidRegistration();
        var response = await service.RegisterAsync(dto);
        var account = Assert.Single(repository.Accounts);
        Assert.Equal(account.Id, response.Id);
        Assert.Equal("DAVID", account.NormalizedUsername);
        Assert.Equal("DAVID@EXAMPLE.COM", account.NormalizedEmail);
        Assert.NotEqual(dto.Password, account.PasswordHash);
        Assert.Equal(PasswordVerificationResult.Success, hasher.VerifyHashedPassword(account, account.PasswordHash, dto.Password));
    }

    [Theory]
    [InlineData("david", "other@example.com")]
    [InlineData("other", "david@example.com")]
    public async Task Register_RejectsDuplicateIgnoringCase(string username, string email)
    {
        var repository = new FakeRepository();
        var service = new AuthenticationService(repository, new PasswordHasher<UserAccount>(), TestTokenService());
        await service.RegisterAsync(ValidRegistration());
        await Assert.ThrowsAsync<DuplicateUserAccountException>(() => service.RegisterAsync(
            new() { Username = username, Email = email, Password = "long-test-password" }));
        Assert.Single(repository.Accounts);
    }

    [Fact]
    public async Task Register_PropagatesConcurrentDuplicate()
    {
        var service = new AuthenticationService(new FakeRepository { SimulateDuplicate = true }, new PasswordHasher<UserAccount>(), TestTokenService());
        await Assert.ThrowsAsync<DuplicateUserAccountException>(() => service.RegisterAsync(ValidRegistration()));
    }

    [Theory]
    [InlineData("", "valid@example.com", "long-test-password")]
    [InlineData("invalid space", "valid@example.com", "long-test-password")]
    [InlineData("valid", "invalid", "long-test-password")]
    [InlineData("valid", "valid@example.com", "short")]
    public async Task Register_RejectsInvalidDataBeforePersisting(string username, string email, string password)
    {
        var repository = new FakeRepository();
        var service = new AuthenticationService(repository, new PasswordHasher<UserAccount>(), TestTokenService());
        await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => service.RegisterAsync(
            new() { Username = username, Email = email, Password = password }));
        Assert.Empty(repository.Accounts);
    }
}
