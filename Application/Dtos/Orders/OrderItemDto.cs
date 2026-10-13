namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// Public projection of a single order line, including the product summary
/// captured at the time the DTO was built.
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the order item.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the ordered product.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the name of the ordered product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SKU of the ordered product.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of units ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price snapshot taken when the order was placed.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets or sets the line total: quantity multiplied by unit price.
    /// </summary>
    public decimal LineTotal { get; set; }
}
