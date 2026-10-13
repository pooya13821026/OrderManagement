using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// Repository contract for <see cref="ApplicationUser"/> entities.
/// </summary>
public interface IUserRepository : IRepository<ApplicationUser, Guid>
{
    /// <summary>
    /// Gets a user by their unique email address, or <c>null</c> when no such user exists.
    /// </summary>
    /// <param name="email">The email address to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their unique user name, or <c>null</c> when no such user exists.
    /// </summary>
    /// <param name="userName">The user name to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a user with the given email address already exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
