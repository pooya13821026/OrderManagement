using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

/// <summary>
/// یک ردیف فقط-افزودنی از تاریخچه‌ی تغییرات وضعیت یک سفارش: هر گذار موفق
/// (<c>FromStatus → ToStatus</c>) یک ردیف ثبت می‌کند و هرگز ویرایش یا حذف نمی‌شود
/// (گزارش‌پذیری audit). حذف سفارش، تاریخچه‌ی آن را هم آبشاری حذف می‌کند چون تاریخچه
/// بخشی از همان aggregate سفارش است و بدون سفارش معنا ندارد.
/// عمداً <c>RowVersion</c> ندارد: داده append-only است و تداخل نوشتن روی آن ممکن نیست.
/// </summary>
public class OrderStatusHistory
{
    /// <summary>
    /// شناسه‌ی یکتای ردیف تاریخچه.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// شناسه‌ی سفارشی که این گذار رخ داده است. همراه با سفارش حذف می‌شود (آبشاری).
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// وضعیت سفارش پیش از این گذار.
    /// </summary>
    public OrderStatus FromStatus { get; set; }

    /// <summary>
    /// وضعیت سفارش پس از این گذار.
    /// </summary>
    public OrderStatus ToStatus { get; set; }

    /// <summary>
    /// زمان‌نشان UTC لحظه‌ی رخداد گذار.
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// شناسه‌ی کاربری که گذار را اعمال کرده است؛ وقتی فراخواننده احراز هویت نشده یا
    /// شناسه‌اش نامعتبر باشد <c>null</c> می‌ماند.
    /// </summary>
    public Guid? ChangedByUserId { get; set; }

    /// <summary>
    /// نقش فراخواننده‌ی گذار (حداکثر ۵۰ نویسه)، برای نمونه <c>Admin</c>.
    /// </summary>
    public string? ChangedByRole { get; set; }

    /// <summary>
    /// دلیل ثبت‌شده‌ی تغییر (حداکثر ۵۰۰ نویسه)؛ عمدتاً برای لغو سفارش استفاده می‌شود.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// پروپرتی ناوبری به سفارش والد.
    /// </summary>
    public Order Order { get; set; } = null!;
}
