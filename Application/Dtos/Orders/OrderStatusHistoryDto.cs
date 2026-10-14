using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// نمای عمومی یک ردیف از تاریخچه‌ی تغییرات وضعیت سفارش؛ فقط-افزودن و فقط خواندنی.
/// </summary>
public class OrderStatusHistoryDto
{
    /// <summary>
    /// شناسه‌ی سفارشی که این گذار رخ داده است را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// وضعیت سفارش پیش از گذار را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public OrderStatus FromStatus { get; set; }

    /// <summary>
    /// وضعیت سفارش پس از گذار را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public OrderStatus ToStatus { get; set; }

    /// <summary>
    /// زمان‌نشان UTC لحظه‌ی رخداد گذار را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// نقش فراخواننده‌ی گذار را می‌گیرد یا تنظیم می‌کند؛ ناموجود <c>null</c> است.
    /// </summary>
    public string? ChangedByRole { get; set; }

    /// <summary>
    /// دلیل ثبت‌شده‌ی تغییر را می‌گیرد یا تنظیم می‌کند؛ ناموجود <c>null</c> است.
    /// </summary>
    public string? Reason { get; set; }
}
