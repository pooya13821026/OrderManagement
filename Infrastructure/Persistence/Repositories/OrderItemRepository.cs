using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IOrderItemRepository"/>. Order items are only ever
/// touched through their parent order, so the generic CRUD operations of
/// <see cref="EfRepository{TEntity,TKey}"/> are sufficient.
/// </summary>
public class OrderItemRepository : EfRepository<OrderItem, Guid>, IOrderItemRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderItemRepository"/> class.
    /// </summary>
    /// <param name="context">The shared database context of the current scope.</param>
    public OrderItemRepository(OrderManagementDbContext context)
        : base(context)
    {
    }
}
