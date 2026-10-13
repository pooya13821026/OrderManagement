using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRefreshTokenRepository"/>. Display reads are not
/// tracked, the refresh-token hash lookup returns a tracked entity because the refresh
/// flow revokes it, and the bulk revoke executes directly in the database so it needs no
/// follow-up save.
/// </summary>
public class RefreshTokenRepository : EfRepository<RefreshToken, Guid>, IRefreshTokenRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenRepository"/> class.
    /// </summary>
    /// <param name="context">The shared database context of the current scope.</param>
    public RefreshTokenRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return null;
        }

        // Tracked on purpose: the refresh flow revokes the presented token (Update) and
        // saves it; a detached copy would collide with an already tracked instance.
        // The authentication service dereferences the owner, so the user is included.
        return await Context.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;

        List<RefreshToken> tokens = await Context.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId
                && token.RevokedAt == null
                && token.ExpiresAt > now)
            .OrderByDescending(token => token.CreatedAt)
            .ToListAsync(cancellationToken);

        return tokens;
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // A single set-based UPDATE instead of loading every token: revocation is a bulk
        // state change that must apply even when the caller never calls SaveChanges, and it
        // still runs inside a transaction opened through the UnitOfWork (same connection).
        await Context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.RevokedAt, DateTime.UtcNow),
                cancellationToken);
    }
}
