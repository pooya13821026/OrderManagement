using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Application.Validators;

/// <summary>
/// Hand-rolled request validators used by the application services.
/// DataAnnotations on the DTOs cover transport-level validation; these checks protect
/// the same invariants when DTOs are constructed in code (for example by background jobs)
/// and add business rules DataAnnotations cannot express.
/// </summary>
public static class RequestValidator
{
    /// <summary>
    /// Ensures the given identifier is not <see cref="Guid.Empty"/>.
    /// </summary>
    /// <param name="id">The identifier to check.</param>
    /// <param name="parameterName">The parameter name used in the exception message.</param>
    /// <exception cref="DomainException">Thrown when the identifier is empty.</exception>
    public static void EnsureNotEmptyId(Guid id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException($"{parameterName} must not be an empty identifier.");
        }
    }

    /// <summary>
    /// Ensures the given text is not null, empty or whitespace.
    /// </summary>
    /// <param name="value">The text to check.</param>
    /// <param name="parameterName">The parameter name used in the exception message.</param>
    /// <exception cref="DomainException">Thrown when the text is null, empty or whitespace.</exception>
    public static void EnsureNotEmpty(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} must not be null or empty.");
        }
    }

    /// <summary>
    /// Validates a single order creation request: customer identifier, at least one item,
    /// non-empty product identifiers and positive quantities.
    /// </summary>
    /// <param name="request">The order request to validate.</param>
    /// <exception cref="DomainException">Thrown when the request is invalid.</exception>
    public static void ValidateOrderRequest(CreateOrderRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        EnsureNotEmptyId(request.CustomerId, nameof(request.CustomerId));

        if (request.Items is null || request.Items.Count == 0)
        {
            throw new DomainException("An order must contain at least one item.");
        }

        foreach (CreateOrderItemRequest item in request.Items)
        {
            if (item is null)
            {
                throw new DomainException("Order items must not be null.");
            }

            EnsureNotEmptyId(item.ProductId, nameof(item.ProductId));

            if (item.Quantity <= 0)
            {
                throw new DomainException($"Order item quantity must be greater than zero (product '{item.ProductId}').");
            }
        }
    }

    /// <summary>
    /// Validates a bulk order request: the list is present, non-empty and does not
    /// exceed <see cref="BulkCreateOrdersRequest.MaxOrders"/> entries; each entry is
    /// validated with <see cref="ValidateOrderRequest"/>.
    /// </summary>
    /// <param name="request">The bulk request to validate.</param>
    /// <exception cref="DomainException">Thrown when the request is invalid.</exception>
    public static void ValidateBulkOrderRequest(BulkCreateOrdersRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Orders is null || request.Orders.Count == 0)
        {
            throw new DomainException("A bulk request must contain at least one order.");
        }

        if (request.Orders.Count > BulkCreateOrdersRequest.MaxOrders)
        {
            throw new DomainException(
                $"A bulk request may contain at most {BulkCreateOrdersRequest.MaxOrders} orders; {request.Orders.Count} were submitted.");
        }

        foreach (CreateOrderRequest orderRequest in request.Orders)
        {
            ValidateOrderRequest(orderRequest);
        }
    }
}
