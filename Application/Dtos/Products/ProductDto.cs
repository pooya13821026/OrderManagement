namespace OrderManagement.Application.Dtos.Products;

/// <summary>
/// Public projection of a product.
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the product.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unit price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the number of units available in stock.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the unique SKU code.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the product was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the last update.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
