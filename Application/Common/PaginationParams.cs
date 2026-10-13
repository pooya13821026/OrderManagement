using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Common;

/// <summary>
/// ورودی‌های استاندارد صفحه‌بندی که در همه‌ی endpoint های فهرست صفحه‌بندی‌شده استفاده می‌شوند.
/// پیش از ارسال مقادیر به مخزن <see cref="Normalize"/> را فراخوانی کنید
/// تا مقادیر خارج از محدوده به جای ایجاد خطا، محدود (clamp) شوند.
/// </summary>
public class PaginationParams
{
    /// <summary>
    /// اندازه‌ی صفحه‌ی پیش‌فرض که وقتی اندازه‌ی صفحه‌ای ارائه نشده (یا نامعتبر باشد) استفاده می‌شود.
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// کران بالای <see cref="PageSize"/>. اندازه‌های درخواستی بالاتر از این مقدار، محدود می‌شوند.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// شماره‌ی صفحه که از ۱ شروع می‌شود را می‌گیرد یا تنظیم می‌کند. مقدار پیش‌فرض ۱ است.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// تعداد اقلام در هر صفحه را می‌گیرد یا تنظیم می‌کند. مقدار پیش‌فرض <see cref="DefaultPageSize"/> است
    /// و به <see cref="MaxPageSize"/> محدود می‌شود.
    /// </summary>
    [Range(1, MaxPageSize)]
    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>
    /// <see cref="PageNumber"/> و <see cref="PageSize"/> را در محدوده‌های معتبر محدود می‌کند:
    /// شماره‌ی صفحه حداقل ۱، اندازه‌ی صفحه بین ۱ و <see cref="MaxPageSize"/>.
    /// </summary>
    public void Normalize()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        if (PageSize < 1)
        {
            PageSize = DefaultPageSize;
        }
        else if (PageSize > MaxPageSize)
        {
            PageSize = MaxPageSize;
        }
    }
}
