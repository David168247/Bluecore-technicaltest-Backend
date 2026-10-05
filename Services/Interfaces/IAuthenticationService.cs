using BluecoreApi.DTOs;
namespace BluecoreApi.Services.Interfaces;
public interface IAuthenticationService
{
    Task<UserAccountResponseDto> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);
}
