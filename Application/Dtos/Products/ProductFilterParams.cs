using OrderManagement.Application.Common;

namespace OrderManagement.Application.Dtos.Products;

/// <summary>
/// ورودی‌های فیلتر برای صفحه‌بندی کاتالوگ محصولات.
/// </summary>
public class ProductFilterParams : PaginationParams
{
    /// <summary>
    /// عبارت جست‌وجوی متنی آزاد اختیاری که روی نام محصول و SKU تطبیق داده می‌شود را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// فیلتر اختیاری پرچم فعال بودن را می‌گیرد یا تنظیم می‌کند؛ <c>null</c> هم محصولات فعال و هم غیرفعال را برمی‌گرداند.
    /// </summary>
    public bool? IsActive { get; set; }
}
