using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// Public projection of an order, including the customer summary and line items.
/// </summary>
public class OrderDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the order.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique human readable order number.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the customer who placed the order.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the full name of the customer who placed the order.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the customer who placed the order.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the current lifecycle status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the order was placed.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Gets or sets the total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the optional shipping address.
    /// </summary>
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Gets or sets the optional free-form notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the last update.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the line items of the order.
    /// </summary>
    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
}
