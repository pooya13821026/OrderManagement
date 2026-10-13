using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// Repository contract for <see cref="Customer"/> entities.
/// </summary>
public interface ICustomerRepository : IRepository<Customer, Guid>
{
    /// <summary>
    /// Gets a customer by their unique email address, or <c>null</c> when no such customer exists.
    /// </summary>
    /// <param name="email">The email address to look up.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether a customer with the given email address already exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a page of customers, optionally filtered by a free-text search term
    /// matched against name and email.
    /// </summary>
    /// <param name="searchTerm">Optional search term; <c>null</c> or empty returns all customers.</param>
    /// <param name="pageNumber">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<PagedEntities<Customer>> GetPagedAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
