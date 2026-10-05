using BluecoreApi.DTOs;
using BluecoreApi.Models;
namespace BluecoreApi.Services.Interfaces;
public interface ITokenService
{
    AuthenticationResponseDto CreateToken(UserAccount account);
}
