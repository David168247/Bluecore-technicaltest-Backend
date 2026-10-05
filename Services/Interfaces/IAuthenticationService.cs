using BluecoreApi.DTOs;
namespace BluecoreApi.Services.Interfaces;
public interface IAuthenticationService
{
    Task<AuthenticationResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<UserAccountResponseDto> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);
}
