namespace BluecoreApi.DTOs;
public sealed record UserAccountResponseDto(Guid Id, string Username, string Email, DateTime CreatedAt);
