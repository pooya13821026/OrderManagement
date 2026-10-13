using System.ComponentModel.DataAnnotations;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// بدنه‌ی درخواست برای گذاردن سفارش در یک وضعیت جدید.
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// وضعیت جدید درخواستی سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    public OrderStatus NewStatus { get; set; }
}
