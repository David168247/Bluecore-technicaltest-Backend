using System.ComponentModel.DataAnnotations;
namespace BluecoreApi.DTOs;

public sealed class LoginDto
{
    [Required, StringLength(254)]
    public string UsernameOrEmail { get; set; } = string.Empty;
    [Required, StringLength(128)]
    public string Password { get; set; } = string.Empty;
}
