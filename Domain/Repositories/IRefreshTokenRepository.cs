using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// Repository contract for <see cref="RefreshToken"/> entities.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    /// <summary>
    /// Gets a stored refresh token by the hash of the presented token,
    /// or <c>null</c> when no such token exists.
    /// </summary>
    /// <param name="tokenHash">The hash of the refresh token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all currently active (not revoked, not expired) refresh tokens of a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes every stored refresh token of the given user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
