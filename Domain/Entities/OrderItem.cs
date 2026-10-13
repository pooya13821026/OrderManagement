namespace OrderManagement.Domain.Entities;

/// <summary>
/// نماینده‌ی یک ردیف از <see cref="Order"/> است.
/// از قیمت محصول در زمان ثبت سفارش، اسنپ‌شات (تصویر لحظه‌ای) نگه‌داری می‌کند.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// شناسه‌ی یکتای ردیف سفارش.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// شناسه‌ی سفارشی که این ردیف به آن تعلق دارد. همراه با سفارش حذف می‌شود (آبشاری).
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// شناسه‌ی محصول سفارش‌داده‌شده.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// تعداد واحدهای سفارش‌داده‌شده. باید بزرگ‌تر از صفر باشد.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// اسنپ‌شات قیمت واحد محصول در لحظه‌ی ثبت سفارش.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// قیمت کل این ردیف را برمی‌گرداند: <see cref="Quantity"/> ضربدر <see cref="UnitPrice"/>.
    /// محاسبه‌ای است و هرگز ذخیره نمی‌شود.
    /// </summary>
    public decimal LineTotal => Quantity * UnitPrice;

    /// <summary>
    /// زمان‌نشان UTC هنگام ایجاد ردیف سفارش.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// توکن همزمانی که توسط Entity Framework مدیریت می‌شود. ناهماهنگی آن نشان‌دهنده‌ی تغییر همزمان است.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// پروپرتی ناوبری به سفارش والد.
    /// </summary>
    public Order Order { get; set; } = null!;

    /// <summary>
    /// پروپرتی ناوبری به محصول مرجع.
    /// </summary>
    public Product Product { get; set; } = null!;
}
