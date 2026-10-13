namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents a single line item of an <see cref="Order"/>.
/// Stores a snapshot of the product price at the time the order was placed.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Unique identifier of the order item.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the order this item belongs to. Deleted with the order (cascade).
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Identifier of the ordered product.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Number of units ordered. Must be greater than zero.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Snapshot of the product unit price at the moment the order was placed.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets the total price of this line: <see cref="Quantity"/> multiplied by <see cref="UnitPrice"/>.
    /// Computed, never persisted.
    /// </summary>
    public decimal LineTotal => Quantity * UnitPrice;

    /// <summary>
    /// UTC timestamp when the order item was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Concurrency token managed by Entity Framework. A mismatch indicates a concurrent modification.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Navigation property to the parent order.
    /// </summary>
    public Order Order { get; set; } = null!;

    /// <summary>
    /// Navigation property to the referenced product.
    /// </summary>
    public Product Product { get; set; } = null!;
}
