using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// بدنه‌ی درخواست برای ایجاد حداکثر ۵۰۰ سفارش در یک عملیات گروهی.
/// </summary>
public class BulkCreateOrdersRequest
{
    /// <summary>
    /// حداکثر تعداد سفارش مجاز در یک درخواست گروهی.
    /// </summary>
    public const int MaxOrders = 500;

    /// <summary>
    /// سفارش‌هایی که باید ایجاد شوند را می‌گیرد یا تنظیم می‌کند. بین ۱ و <see cref="MaxOrders"/> مورد.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(MaxOrders)]
    public List<CreateOrderRequest> Orders { get; set; } = new();
}
