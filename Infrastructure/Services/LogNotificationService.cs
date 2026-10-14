using Microsoft.Extensions.Logging;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی اطلاع‌رسانی فقط-لاگی گذار وضعیت سفارش: برای هر وضعیت جدید یک قالب
/// لاگ ساخت‌یافته فارسی در سطح Information می‌نویسد. عمداً هیچ SMS/ایمیل واقعی نمی‌فرستد؛
/// جایگزین کردن کانال واقعی فقط با عوض کردن این پیاده‌سازی در DI انجام می‌شود.
/// </summary>
public sealed class LogNotificationService : INotificationService
{
    private readonly ILogger<LogNotificationService> _logger;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="LogNotificationService"/> می‌سازد.
    /// </summary>
    /// <param name="logger">لاگر ساخت‌یافته‌ی برنامه.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="logger"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public LogNotificationService(ILogger<LogNotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task NotifyOrderStatusChangedAsync(
        Guid orderId,
        string orderNumber,
        OrderStatus oldStatus,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        // قالب لاگ بر اساس وضعیت جدید انتخاب می‌شود تا پیام هر گذار به زبان دامنه
        // (تأیید/ارسال/تحویل/لغو) باشد و مقادیر ساخت‌یافته برای جست‌وجو در لاگ بمانند.
        switch (newStatus)
        {
            case OrderStatus.Confirmed:
                _logger.LogInformation(
                    "سفارش {OrderNumber} ({OrderId}) تأیید شد؛ از {OldStatus} به {NewStatus}.",
                    orderNumber,
                    orderId,
                    oldStatus,
                    newStatus);
                break;
            case OrderStatus.Shipped:
                _logger.LogInformation(
                    "سفارش {OrderNumber} ({OrderId}) ارسال شد؛ از {OldStatus} به {NewStatus}.",
                    orderNumber,
                    orderId,
                    oldStatus,
                    newStatus);
                break;
            case OrderStatus.Delivered:
                _logger.LogInformation(
                    "سفارش {OrderNumber} ({OrderId}) تحویل داده شد؛ از {OldStatus} به {NewStatus}.",
                    orderNumber,
                    orderId,
                    oldStatus,
                    newStatus);
                break;
            case OrderStatus.Cancelled:
                _logger.LogInformation(
                    "سفارش {OrderNumber} ({OrderId}) لغو شد؛ از {OldStatus} به {NewStatus}.",
                    orderNumber,
                    orderId,
                    oldStatus,
                    newStatus);
                break;
            default:
                _logger.LogInformation(
                    "وضعیت سفارش {OrderNumber} ({OrderId}) تغییر کرد؛ از {OldStatus} به {NewStatus}.",
                    orderNumber,
                    orderId,
                    oldStatus,
                    newStatus);
                break;
        }

        return Task.CompletedTask;
    }
}
