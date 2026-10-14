using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

/// <summary>
/// نماینده‌ی یک سفارش مشتری است که شامل یک یا چند ردیف سفارش می‌شود.
/// شماره سفارش قالب <c>ORD-yyyyMMdd-xxxxxx</c> را دارد و یکتاست.
/// </summary>
public class Order
{
    /// <summary>
    /// شناسه‌ی یکتای سفارش.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// شناسه‌ی مشتری‌ای که سفارش را ثبت کرده است.
    /// </summary>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// شماره‌ی یکتای سفارش به‌صورت خوانا، با قالب <c>ORD-yyyyMMdd-xxxxxx</c>.
    /// </summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>
    /// وضعیت جاری چرخه‌ی حیات سفارش.
    /// </summary>
    public OrderStatus Status { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ثبت سفارش.
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// مجموع کل ردیف‌ها در زمان ثبت سفارش.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// آدرس ارسال اختیاری؛ ممکن است با آدرس مشتری متفاوت باشد.
    /// </summary>
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// یادداشت‌های آزاد اختیاری که به سفارش پیوست شده‌اند.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد رکورد سفارش.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC آخرین به‌روزرسانی رکورد سفارش.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام تأیید سفارش؛ قبل از تأیید <c>null</c> است.
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام ارسال سفارش؛ قبل از ارسال <c>null</c> است.
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام تحویل سفارش؛ قبل از تحویل <c>null</c> است.
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// زمان‌نشان UTC هنگام لغو سفارش؛ قبل از لغو <c>null</c> است.
    /// </summary>
    public DateTime? CancelledAt { get; set; }

    /// <summary>
    /// دلیل لغو سفارش (حداکثر ۵۰۰ نویسه)؛ فقط هنگام لغو مقدار می‌گیرد و در غیر این صورت <c>null</c> است.
    /// </summary>
    public string? CancelReason { get; set; }

    /// <summary>
    /// توکن همزمانی که توسط Entity Framework مدیریت می‌شود. ناهماهنگی آن نشان‌دهنده‌ی تغییر همزمان است.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// پروپرتی ناوبری به مشتری‌ای که سفارش را ثبت کرده است.
    /// </summary>
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// مجموعه‌ی اقلام موجود در این سفارش.
    /// </summary>
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>
    /// تاریخچه‌ی تغییرات وضعیت این سفارش (فقط افزودنی؛ هر گذار یک ردیف ثبت می‌شود).
    /// عضویت‌ها با حذف سفارش آبشاری حذف می‌شوند چون تاریخچه بخشی از همان تراکنش سفارش است.
    /// </summary>
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}
