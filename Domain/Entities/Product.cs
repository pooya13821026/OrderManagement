namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents a sellable product. SKU is unique across all products.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier of the product.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional long description of the product.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unit price of the product in the store currency.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Current number of units available in stock.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Stock keeping unit code. Unique, used for product lookups and duplicate checks.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the product is active and can be ordered. Defaults to <c>true</c>.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// UTC timestamp when the product record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp of the last update of the product record.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token managed by Entity Framework. A mismatch indicates a concurrent modification.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
