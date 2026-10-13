using OrderManagement.Domain.Entities;

namespace OrderManagement.Domain.Repositories;

/// <summary>
/// Repository contract for <see cref="OrderItem"/> entities.
/// </summary>
public interface IOrderItemRepository : IRepository<OrderItem, Guid>
{
}
