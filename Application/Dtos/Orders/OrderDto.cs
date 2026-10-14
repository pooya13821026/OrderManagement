using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// نمای عمومی یک سفارش، شامل خلاصه‌ی مشتری و ردیف‌های سفارش.
/// </summary>
public class OrderDto
{
    /// <summary>
    /// شناسه‌ی یکتای سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// شماره‌ی یکتای خوانای سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// شناسه‌ی مشتری‌ای که سفارش را ثبت کرده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// نام کامل مشتری‌ای که سفارش را ثبت کرده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// آدرس ایمیل مشتری‌ای که سفارش را ثبت کرده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string CustomerEmail { get; set; } = string.Empty;

    /// <summary>
    /// وضعیت جاری چرخه‌ی حیات سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ثبت سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// مبلغ کل سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// آدرس ارسال اختیاری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// یادداشت‌های آزاد اختیاری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین به‌روزرسانی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام تأیید سفارش را می‌گیرد یا تنظیم می‌کند؛ قبل از تأیید <c>null</c> است.
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ارسال سفارش را می‌گیرد یا تنظیم می‌کند؛ قبل از ارسال <c>null</c> است.
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام تحویل سفارش را می‌گیرد یا تنظیم می‌کند؛ قبل از تحویل <c>null</c> است.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام لغو سفارش را می‌گیرد یا تنظیم می‌کند؛ قبل از لغو <c>null</c> است.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// دلیل لغو سفارش را می‌گیرد یا تنظیم می‌کند؛ برای سفارش‌های غیرلغوشده <c>null</c> است.
    /// </summary>
    public string? CancelReason { get; set; }

    /// <summary>
    /// ردیف‌های تاریخچه‌ی تغییرات وضعیت (مرتب‌شده بر اساس زمان گذار) را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public IReadOnlyList<OrderStatusHistoryDto> StatusHistory { get; set; } = Array.Empty<OrderStatusHistoryDto>();

    /// <summary>
    /// ردیف‌های سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public IReadOnlyList<OrderItemDto> Items { get; set; } = Array.Empty<OrderItemDto>();
}
