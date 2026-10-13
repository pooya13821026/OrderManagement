using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// قرارداد مخزن برای موجودیت‌های <see cref="OrderItem"/>.
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem, Guid>
{
}
