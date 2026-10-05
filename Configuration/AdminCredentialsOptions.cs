namespace BluecoreApi.Configuration;
public class AdminCredentialsOptions
{
    public const string SectionName = "AdminCredentials";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}