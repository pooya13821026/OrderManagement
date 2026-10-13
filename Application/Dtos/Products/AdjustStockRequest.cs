using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Products;

/// <summary>
/// بدنه‌ی درخواست برای تنظیم موجودی یک محصول. مقدار مثبت یعنی شارژ (افزایش) و
/// مقدار منفی یعنی برداشت (کاهش)؛ صفر مجاز نیست.
/// </summary>
public class AdjustStockRequest
{
    /// <summary>
    /// میزان تغییر موجودی را می‌گیرد یا تنظیم می‌کند. مثبت برای افزایش و منفی برای کاهش.
    /// </summary>
    [Required]
    public int QuantityChange { get; set; }
}
