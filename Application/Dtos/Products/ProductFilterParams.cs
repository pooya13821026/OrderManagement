using OrderManagement.Application.Common;

namespace OrderManagement.Application.Dtos.Products;

/// <summary>
/// Filter inputs for paging through the product catalog.
/// </summary>
public class ProductFilterParams : PaginationParams
{
    /// <summary>
    /// Gets or sets an optional free-text search term matched against product name and SKU.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets an optional active flag filter; <c>null</c> returns both active and inactive products.
    /// </summary>
    public bool? IsActive { get; set; }
}
