namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// سرنوشت یک سفارش واحد درون یک درخواست ایجاد گروهی.
/// </summary>
public class BulkOrderResultItem
{
    /// <summary>
    /// اندیس مبتنی بر صفر سفارش در فهرست درخواست ارسالی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// مقداری که نشان می‌دهد این سفارش با موفقیت ایجاد شد را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// شناسه‌ی سفارش ایجادشده، یا <c>null</c> در صورت شکست ایجاد را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid? OrderId { get; set; }

    /// <summary>
    /// شماره‌ی سفارش ایجادشده، یا <c>null</c> در صورت شکست ایجاد را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? OrderNumber { get; set; }

    /// <summary>
    /// پیام خطا در صورت شکست ایجاد، یا <c>null</c> در صورت موفقیت را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
