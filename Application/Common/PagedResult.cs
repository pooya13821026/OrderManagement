namespace OrderManagement.Application.Common;

/// <summary>
/// یک صفحه از نتیجه‌ی پرس‌وجوی صفحه‌بندی‌شده را به همراه فراداده‌ی صفحه‌بندی نشان می‌دهد.
/// </summary>
/// <typeparam name="T">نوع اقلام موجود در صفحه.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="PagedResult{T}"/> می‌سازد.
    /// </summary>
    /// <param name="items">اقلام تعلق‌دار به صفحه‌ی درخواست‌شده.</param>
    /// <param name="pageNumber">شماره‌ی صفحه‌ی درخواستی که از ۱ شروع می‌شود.</param>
    /// <param name="pageSize">اندازه‌ی صفحه‌ی درخواست‌شده.</param>
    /// <param name="totalCount">تعداد کل اقلام منطبق در همه‌ی صفحات.</param>
    public PagedResult(IReadOnlyList<T> items, int pageNumber, int pageSize, int totalCount)
    {
        Items = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = pageSize > 0
            ? (int)Math.Ceiling(totalCount / (double)pageSize)
            : 0;
    }

    /// <summary>
    /// اقلام تعلق‌دار به صفحه‌ی درخواست‌شده را برمی‌گرداند.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// شماره‌ی صفحه‌ی درخواستی که از ۱ شروع می‌شود را برمی‌گرداند.
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// اندازه‌ی صفحه‌ی درخواست‌شده را برمی‌گرداند.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// تعداد کل اقلام منطبق در همه‌ی صفحات را برمی‌گرداند.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// تعداد کل صفحات موجود برای اندازه‌ی صفحه‌ی داده‌شده را برمی‌گرداند.
    /// </summary>
    public int TotalPages { get; }
}
