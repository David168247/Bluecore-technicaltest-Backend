using System.ComponentModel.DataAnnotations;
using BluecoreApi.DTOs;
using BluecoreApi.Exceptions;
using BluecoreApi.Models;
using BluecoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
namespace BluecoreApi.Services.Implementations;

public sealed class AuthenticationService(
    IUserAccountRepository repository,
    IPasswordHasher<UserAccount> passwordHasher,
    ITokenService tokenService) : IAuthenticationService
{
    // Equal-cost password verification for unknown accounts avoids a cheap username enumeration path.
    private static readonly UserAccount DummyAccount = new();
    private static readonly string DummyPasswordHash =
        new PasswordHasher<UserAccount>().HashPassword(DummyAccount, Guid.NewGuid().ToString());

    public async Task<AuthenticationResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var account = await repository.FindByIdentifierAsync(dto.UsernameOrEmail.Trim().ToUpperInvariant(), cancellationToken);
        var verification = passwordHasher.VerifyHashedPassword(
            account ?? DummyAccount, account?.PasswordHash ?? DummyPasswordHash, dto.Password);
        if (account is null || verification == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            await repository.UpdatePasswordHashAsync(account.Id, passwordHasher.HashPassword(account, dto.Password), cancellationToken);
        return tokenService.CreateToken(account);
    }

    public async Task<UserAccountResponseDto> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var username = dto.Username.Trim();
        var email = dto.Email.Trim();
        var normalizedUsername = username.ToUpperInvariant();
        var normalizedEmail = email.ToUpperInvariant();
        if (await repository.ExistsAsync(normalizedUsername, normalizedEmail, cancellationToken))
            throw new DuplicateUserAccountException();

        var account = new UserAccount
        {
            Username = username,
            Email = email,
            NormalizedUsername = normalizedUsername,
            NormalizedEmail = normalizedEmail
        };
        account.PasswordHash = passwordHasher.HashPassword(account, dto.Password);
        await repository.AddAsync(account, cancellationToken);
        return new(account.Id, account.Username, account.Email, account.CreatedAt);
    }
}
