namespace OrderManagement.Domain.Enums;

/// <summary>
/// وضعیت چرخه‌ی حیات یک سفارش. در پایگاه داده به‌صورت byte ذخیره می‌شود.
/// </summary>
public enum OrderStatus : byte
{
    /// <summary>
    /// سفارش ثبت شده اما هنوز تأیید نشده است. موجودی هنوز رزرو نشده است.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// سفارش تأیید شده و موجودی کاهش یافته است.
    /// </summary>
    Confirmed = 2,

    /// <summary>
    /// سفارش به حمل‌کننده تحویل داده شده است.
    /// </summary>
    Shipped = 3,

    /// <summary>
    /// سفارش به مشتری تحویل داده شده است. وضعیت نهایی موفق.
    /// </summary>
    Delivered = 4,

    /// <summary>
    /// سفارش لغو شده است. وضعیت نهایی؛ فقط از Pending یا Confirmed مجاز است.
    /// </summary>
    Cancelled = 5
}
