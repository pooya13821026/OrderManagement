using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// همه‌ی مخزن‌ها را به همراه یک SaveChanges واحد و مرز تراکنش جمع می‌کند،
/// تا یک سرویس بتواند چند مخزن را به‌صورت اتمیک هماهنگ کند بدون اینکه EF Core را بشناسد.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// مخزن موجودیت‌های مشتری را برمی‌گرداند.
    /// </summary>
    ICustomerRepository Customers { get; }

    /// <summary>
    /// مخزن موجودیت‌های دسته‌بندی را برمی‌گرداند.
    /// </summary>
    ICategoryRepository Categories { get; }

    /// <summary>
    /// مخزن موجودیت‌های محصول را برمی‌گرداند.
    /// </summary>
    IProductRepository Products { get; }

    /// <summary>
    /// مخزن موجودیت‌های سفارش را برمی‌گرداند.
    /// </summary>
    IOrderRepository Orders { get; }

    /// <summary>
    /// مخزن موجودیت‌های ردیف سفارش را برمی‌گرداند.
    /// </summary>
    IOrderItemRepository OrderItems { get; }

    /// <summary>
    /// مخزن موجودیت‌های کاربر برنامه را برمی‌گرداند.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// مخزن موجودیت‌های توکن refresh را برمی‌گرداند.
    /// </summary>
    IRefreshTokenRepository RefreshTokens { get; }

    /// <summary>
    /// همه‌ی تغییرات معلق انجام‌شده از طریق مخزن‌ها را ذخیره می‌کند.
    /// </summary>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>تعداد ورودی‌های وضعیت نوشته‌شده در ذخیره‌گاه.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// یک تراکنش پایگاه داده‌ی جدید آغاز می‌کند. باید با
    /// <see cref="CommitAsync"/> یا <see cref="RollbackAsync"/> جفت شود.
    /// </summary>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// تراکنش پایگاه داده‌ی فعلی را commit می‌کند.
    /// </summary>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// تراکنش پایگاه داده‌ی فعلی را rollback می‌کند.
    /// </summary>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
