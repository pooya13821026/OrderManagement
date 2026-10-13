namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// نتیجه‌ی یک عملیات ایجاد گروهی سفارش، شامل نتیجه‌ی هر سفارش.
/// </summary>
public class BulkCreateOrdersResponse
{
    /// <summary>
    /// تعداد سفارش‌هایی که با موفقیت ایجاد شدند را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public int SuccessfulCount { get; set; }

    /// <summary>
    /// تعداد سفارش‌هایی که در اعتبارسنجی یا ذخیره‌سازی ناموفق بودند را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// نتایج هر سفارش را به همان ترتیب فهرست ورودی می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public IReadOnlyList<BulkOrderResultItem> Results { get; set; } = Array.Empty<BulkOrderResultItem>();
}
