using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Products;

/// <summary>
/// Payload for creating a new product.
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// Gets or sets the display name of the product.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description of the product.
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the unit price of the product.
    /// </summary>
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the initial stock quantity of the product.
    /// </summary>
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the unique SKU code of the product.
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the product is active. Defaults to <c>true</c>.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
