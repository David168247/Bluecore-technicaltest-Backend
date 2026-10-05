using BluecoreApi.Data;
using BluecoreApi.Exceptions;
using BluecoreApi.Models;
using BluecoreApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace BluecoreApi.Services.Implementations;

public sealed class UserAccountRepository(AppDbContext context) : IUserAccountRepository
{
    public Task<bool> ExistsAsync(string normalizedUsername, string normalizedEmail, CancellationToken cancellationToken) =>
        context.UserAccounts.AsNoTracking().AnyAsync(
            x => x.NormalizedUsername == normalizedUsername || x.NormalizedEmail == normalizedEmail, cancellationToken);

    public Task<UserAccount?> FindByIdentifierAsync(string normalizedIdentifier, CancellationToken cancellationToken) =>
        context.UserAccounts.AsNoTracking().SingleOrDefaultAsync(
            x => x.NormalizedUsername == normalizedIdentifier || x.NormalizedEmail == normalizedIdentifier, cancellationToken);

    public Task UpdatePasswordHashAsync(Guid id, string passwordHash, CancellationToken cancellationToken) =>
        context.UserAccounts.Where(x => x.Id == id).ExecuteUpdateAsync(
            setters => setters.SetProperty(x => x.PasswordHash, passwordHash), cancellationToken);

    public async Task AddAsync(UserAccount account, CancellationToken cancellationToken)
    {
        context.UserAccounts.Add(account);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg &&
            pg.ConstraintName is "ux_user_accounts_normalized_username" or "ux_user_accounts_normalized_email")
        {
            context.Entry(account).State = EntityState.Detached;
            throw new DuplicateUserAccountException();
        }
    }
}
