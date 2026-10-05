using System.ComponentModel.DataAnnotations;
namespace BluecoreApi.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    [Required]
    public string Key { get; set; } = string.Empty;
    [Required]
    public string Issuer { get; set; } = string.Empty;
    [Required]
    public string Audience { get; set; } = string.Empty;
    [Range(1, 120)]
    public int ExpirationMinutes { get; set; } = 60;
}
