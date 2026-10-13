using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/>. The email lookup feeds the
/// sign-in flow (which updates and saves the user) and therefore returns tracked entities;
/// the user-name lookup is display-only and runs with <c>AsNoTracking</c>.
/// </summary>
public class UserRepository : EfRepository<ApplicationUser, Guid>, IUserRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserRepository"/> class.
    /// </summary>
    /// <param name="context">The shared database context of the current scope.</param>
    public UserRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        // Tracked on purpose: the sign-in flow updates LastLoginAt on the returned user
        // and saves it; a detached copy would collide with an already tracked instance.
        return await Context.Users
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        return await Context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.UserName == userName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return await Context.Users
            .AnyAsync(user => user.Email == email, cancellationToken);
    }
}
