using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// بدنه‌ی درخواست برای ایجاد یک سفارش جدید.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// شناسه‌ی مشتری‌ای که سفارش را ثبت می‌کند را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>
    /// ردیف‌های سفارش را می‌گیرد یا تنظیم می‌کند. حداقل یک ردیف الزامی است.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; set; } = new();

    /// <summary>
    /// آدرس ارسال اختیاری برای این سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [MaxLength(500)]
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// یادداشت‌های آزاد اختیاری برای این سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
