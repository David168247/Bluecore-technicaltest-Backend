using BluecoreApi.Models;
namespace BluecoreApi.Services.Interfaces;

public interface IUserAccountRepository
{
    Task<bool> ExistsAsync(string normalizedUsername, string normalizedEmail, CancellationToken cancellationToken);
    Task<UserAccount?> FindByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken);
    Task UpdatePasswordHashAsync(Guid id, string passwordHash, CancellationToken cancellationToken);
    Task AddAsync(UserAccount account, CancellationToken cancellationToken);
}
