using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// قرارداد اطلاع‌رسانی رویدادهای چرخه‌ی حیات سفارش. پیاده‌سازی در Infrastructure قرار
/// دارد و در حال حاضر فقط لاگ ساخت‌یافته می‌نویسد؛ اینجا فقط انتزاع است تا سرویس
/// برنامه به پیام‌رسان/ایمیل وابسته نشود و unit-test بماند. آرگومان‌ها scalar هستند
/// (نه موجودیت ردیابی‌شده‌ی سفارش) تا از coupling به EF و از افشای ناخواسته داده جلوگیری شود.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// تغییر وضعیت یک سفارش را به شیوه‌ی اطلاع‌رسانی پیاده‌سازی‌شده (در حال حاضر لاگ)
    /// اعلام می‌کند. باید پس از commit تراکنش فراخوانی شود تا شکست آن وضعیت کسب‌وکار را برنگرداند.
    /// </summary>
    /// <param name="orderId">شناسه‌ی سفارش.</param>
    /// <param name="orderNumber">شماره‌ی خوانای سفارش.</param>
    /// <param name="oldStatus">وضعیت سفارش پیش از گذار.</param>
    /// <param name="newStatus">وضعیت سفارش پس از گذار.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>یک <see cref="Task"/> که پس از پایان اطلاع‌رسانی تکمیل می‌شود.</returns>
    Task NotifyOrderStatusChangedAsync(
        Guid orderId,
        string orderNumber,
        OrderStatus oldStatus,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default);
}
