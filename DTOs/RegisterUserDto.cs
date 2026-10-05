using System.ComponentModel.DataAnnotations;
namespace BluecoreApi.DTOs;

public sealed class RegisterUserDto
{
    [Required, StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"[a-zA-Z0-9_.-]+", ErrorMessage = "El usuario solo admite letras ASCII, números, punto, guion y guion bajo.")]
    public string Username { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string Password { get; set; } = string.Empty;
}
