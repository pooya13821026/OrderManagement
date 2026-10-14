using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// پیاده‌سازی EF Core برای مخزن عمومی <see cref="OrderStatusHistory"/>؛ فقط یک
/// زیرکلاس نازک از <see cref="EfRepository{TEntity,TKey}"/> است چون
/// <c>EfRepository</c> انتزاعی است و DI فقط نوع‌های کلاسیِ قابل ساخت را می‌تواند ثبت کند.
/// تاریخچه append-only است؛ سرویس برنامه فقط از <c>AddAsync</c> استفاده می‌کند.
/// </summary>
public class OrderStatusHistoryRepository : EfRepository<OrderStatusHistory, Guid>
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="OrderStatusHistoryRepository"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
    public OrderStatusHistoryRepository(OrderManagementDbContext context)
        : base(context)
    {
    }
}
