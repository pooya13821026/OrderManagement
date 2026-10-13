using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// پیاده‌سازی EF Core برای <see cref="IOrderItemRepository"/>. ردیف‌های سفارش فقط از طریق
/// سفارش والدشان لمس می‌شوند، بنابراین عملیات CRUD عمومی
/// <see cref="EfRepository{TEntity,TKey}"/> کافی است.
/// </summary>
public class OrderItemRepository : EfRepository<OrderItem, Guid>, IOrderItemRepository
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="OrderItemRepository"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
    public OrderItemRepository(OrderManagementDbContext context)
        : base(context)
    {
    }
}
