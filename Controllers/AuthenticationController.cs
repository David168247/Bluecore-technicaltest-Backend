using BluecoreApi.DTOs;
using BluecoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace BluecoreApi.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[EnableRateLimiting("authentication")]
public sealed class AuthenticationController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserAccountResponseDto>> Register(RegisterUserDto dto, CancellationToken cancellationToken)
    {
        var account = await authenticationService.RegisterAsync(dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, account);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponseDto>> Login(LoginDto dto, CancellationToken cancellationToken) =>
        Ok(await authenticationService.LoginAsync(dto, cancellationToken));
}
