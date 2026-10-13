using OrderManagement.Application.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// ورودی‌های فیلتر و صفحه‌بندی برای مرور صفحه‌بندی‌شده‌ی سفارش‌ها.
/// </summary>
public class OrderFilterParams : PaginationParams
{
    /// <summary>
    /// فیلتر اختیاری شناسه‌ی مشتری را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// فیلتر اختیاری وضعیت سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// کران پایین اختیاری و شامل بر تاریخ سفارش (UTC) را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// کران بالای اختیاری و شامل بر تاریخ سفارش (UTC) را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// عبارت جست‌وجوی متنی آزاد اختیاری که روی شماره سفارش
    /// و آدرس ارسال تطبیق داده می‌شود را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// ستون مرتب‌سازی اختیاری: <c>OrderNumber</c>، <c>OrderDate</c>
    /// یا <c>TotalAmount</c>. مقدار <c>null</c> به تاریخ سفارش برمی‌گردد.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// مقداری که نشان می‌دهد مرتب‌سازی باید نزولی باشد
    /// به جای صعودی را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public bool SortDescending { get; set; }
}
