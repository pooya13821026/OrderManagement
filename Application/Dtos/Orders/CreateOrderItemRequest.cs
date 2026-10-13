using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Orders;

/// <summary>
/// بدنه‌ای که یک ردیف از یک سفارش جدید را توصیف می‌کند.
/// </summary>
public class CreateOrderItemRequest
{
    /// <summary>
    /// شناسه‌ی محصولی که سفارش داده می‌شود را می‌گیرد یا تنظیم می‌کند.
    /// </summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>
    /// تعداد واحدهای درخواستی را می‌گیرد یا تنظیم می‌کند. باید بزرگ‌تر از صفر باشد.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
