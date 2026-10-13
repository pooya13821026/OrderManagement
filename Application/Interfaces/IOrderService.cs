using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// Application service contract for order lifecycle operations.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Creates a new order in the <see cref="OrderStatus.Pending"/> status after validating
    /// the customer, the ordered products and the quantities, snapshotting unit prices
    /// and computing line totals and the order total.
    /// </summary>
    /// <param name="request">The order creation payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created order as a DTO.</returns>
    Task<OrderDto> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single order by identifier, including customer and items.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The order as a DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Thrown when the order does not exist.</exception>
    Task<OrderDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a filtered, sorted and paginated page of orders.
    /// </summary>
    /// <param name="filter">The filter, sorting and pagination inputs.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A page of order DTOs.</returns>
    Task<PagedResult<OrderDto>> GetPagedAsync(OrderFilterParams filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transitions an order to a new status, enforcing the forward-only transition rules.
    /// On confirmation, stock is validated for all items first and only then decremented;
    /// cancelling requires the <c>Admin</c> role.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="newStatus">The requested new status.</param>
    /// <param name="requesterRole">The role of the calling user (from the API layer's claims).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated order as a DTO.</returns>
    Task<OrderDto> UpdateStatusAsync(
        Guid orderId,
        OrderStatus newStatus,
        string requesterRole,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an order after verifying it exists. Role checks are enforced by the API layer.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Thrown when the order does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates many orders in one transaction, recording a per-order success or failure
    /// result so that partial failures are reported instead of discarded.
    /// </summary>
    /// <param name="request">The bulk creation payload (at most 500 orders).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The bulk response with per-order results.</returns>
    Task<BulkCreateOrdersResponse> BulkCreateAsync(
        BulkCreateOrdersRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the requested status transition is allowed by the business rules:
    /// forward-only single step <c>Pending → Confirmed → Shipped → Delivered</c>,
    /// plus <c>Cancelled</c> from <c>Pending</c> or <c>Confirmed</c>.
    /// </summary>
    /// <param name="currentStatus">The status the order currently has.</param>
    /// <param name="requestedStatus">The status that was requested.</param>
    /// <returns><c>true</c> when the transition is allowed; otherwise <c>false</c>.</returns>
    bool CanTransition(OrderStatus currentStatus, OrderStatus requestedStatus);
}
