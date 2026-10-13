using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// Payload for creating up to 500 orders in a single bulk operation.
/// </summary>
public class BulkCreateOrdersRequest
{
    /// <summary>
    /// Maximum number of orders accepted in one bulk request.
    /// </summary>
    public const int MaxOrders = 500;

    /// <summary>
    /// Gets or sets the orders to create. Between 1 and <see cref="MaxOrders"/> entries.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(MaxOrders)]
    public List<CreateOrderRequest> Orders { get; set; } = new();
}
