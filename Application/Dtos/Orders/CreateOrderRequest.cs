using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// Payload for creating a new order.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// Gets or sets the identifier of the customer placing the order.
    /// </summary>
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the line items of the order. At least one item is required.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional shipping address for this order.
    /// </summary>
    [MaxLength(500)]
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Gets or sets the optional free-form notes for this order.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
