namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// هنگامی پرتاب می‌شود که سفارش به دلیل ناکافی بودن موجودی یکی از اقلامش قابل تأیید نباشد.
/// با پرتاب شدن این استثنا، هیچ موجودی کاهش داده نمی‌شود.
/// </summary>
public class InsufficientStockException : DomainException
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="InsufficientStockException"/> می‌سازد.
    /// </summary>
    /// <param name="productId">شناسه‌ی محصولی که موجودی‌اش ناکافی است.</param>
    /// <param name="available">تعداد واحدهای موجود در انبار.</param>
    /// <param name="requested">تعداد واحدهای درخواست‌شده.</param>
    public InsufficientStockException(Guid productId, int available, int requested)
        : base($"موجودی محصول '{productId}' کافی نیست: {available} موجود، {requested} درخواست شده.")
    {
        ProductId = productId;
        Available = available;
        Requested = requested;
    }

    /// <summary>
    /// شناسه‌ی محصولی که موجودی‌اش ناکافی است را برمی‌گرداند.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// تعداد واحدهای موجود در انبار را برمی‌گرداند.
    /// </summary>
    public int Available { get; }

    /// <summary>
    /// تعداد واحدهای درخواست‌شده را برمی‌گرداند.
    /// </summary>
    public int Requested { get; }
}
