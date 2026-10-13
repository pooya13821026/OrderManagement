using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Customers;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// Application service contract for customer management.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Creates a new customer after verifying the email address is not already taken.
    /// </summary>
    /// <param name="request">The customer creation payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created customer as a DTO.</returns>
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by identifier.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The customer as a DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Thrown when the customer does not exist.</exception>
    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a filtered and paginated page of customers.
    /// </summary>
    /// <param name="parameters">The pagination inputs.</param>
    /// <param name="searchTerm">Optional free-text search term matched against name and email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A page of customer DTOs.</returns>
    Task<PagedResult<CustomerDto>> GetPagedAsync(
        PaginationParams parameters,
        string? searchTerm,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing customer, keeping the email address unique.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="request">The customer update payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated customer as a DTO.</returns>
    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a customer after verifying they exist.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Thrown when the customer does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
