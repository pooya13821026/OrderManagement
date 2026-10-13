using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// Payload describing a single line of a new order.
/// </summary>
public class CreateOrderItemRequest
{
    /// <summary>
    /// Gets or sets the identifier of the product being ordered.
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// Gets or sets the number of units to order. Must be greater than zero.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
