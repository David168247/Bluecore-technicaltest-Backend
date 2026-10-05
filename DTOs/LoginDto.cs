using System.ComponentModel.DataAnnotations;

namespace BluecoreApi.DTOs;

public sealed class LoginDto
{
    [Required, StringLength(250)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Password { get; set; } = string.Empty;
}
