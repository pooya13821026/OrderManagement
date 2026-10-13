namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// نمای عمومی یک ردیف سفارش، شامل خلاصه‌ی محصول
/// که در زمان ساخت DTO ثبت شده است.
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// شناسه‌ی یکتای ردیف سفارش را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// شناسه‌ی محصول سفارش‌داده‌شده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// نام محصول سفارش‌داده‌شده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// کد SKU محصول سفارش‌داده‌شده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// تعداد واحدهای سفارش‌داده‌شده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// اسنپ‌شات قیمت واحد که در زمان ثبت سفارش گرفته شده را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// قیمت کل ردیف: تعداد ضربدر قیمت واحد را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    public decimal LineTotal { get; set; }
}
