namespace BluecoreApi.DTOs;
public sealed record AuthenticationResponseDto(string AccessToken, string TokenType, DateTime ExpiresAt, UserAccountResponseDto User);
