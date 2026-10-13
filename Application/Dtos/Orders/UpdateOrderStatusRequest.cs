using System.ComponentModel.DataAnnotations;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// Payload for transitioning an order to a new status.
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// Gets or sets the requested new status of the order.
    /// </summary>
    [Required]
    public OrderStatus NewStatus { get; set; }
}
