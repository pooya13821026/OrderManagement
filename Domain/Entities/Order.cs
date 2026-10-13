using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents a customer order containing one or more order items.
/// The order number has the format <c>ORD-yyyyMMdd-xxxxxx</c> and is unique.
/// </summary>
public class Order
{
    /// <summary>
    /// Unique identifier of the order.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the customer who placed the order.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Human readable unique order number, format <c>ORD-yyyyMMdd-xxxxxx</c>.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current lifecycle status of the order.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// UTC timestamp when the order was placed.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Sum of all item line totals at the time the order was placed.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Optional shipping address, may differ from the customer's address.
    /// </summary>
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Optional free-form notes attached to the order.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// UTC timestamp when the order record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp of the last update of the order record.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token managed by Entity Framework. A mismatch indicates a concurrent modification.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Navigation property to the customer who placed the order.
    /// </summary>
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// Collection of items contained in this order.
    /// </summary>
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
