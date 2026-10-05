using System.ComponentModel.DataAnnotations;
using BluecoreApi.DTOs;
using BluecoreApi.Exceptions;
using BluecoreApi.Models;
using BluecoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
namespace BluecoreApi.Services.Implementations;

public sealed class AuthenticationService(
    IUserAccountRepository repository,
    IPasswordHasher<UserAccount> passwordHasher) : IAuthenticationService
{
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
