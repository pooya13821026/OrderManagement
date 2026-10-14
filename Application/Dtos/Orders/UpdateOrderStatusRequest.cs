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

    /// <summary>
    /// دلیل تغییر وضعیت (اختیاری، حداکثر ۵۰۰ نویسه) را می‌گیرد یا تنظیم می‌کند.
    /// فقط هنگام لغو سفارش (<see cref="OrderStatus.Cancelled"/>) معنا دارد؛ در گذارهای
    /// دیگر نادیده گرفته می‌شود.
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; set; }
}
