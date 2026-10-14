using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// repository های scoped را به‌همراه یک <see cref="OrderManagementDbContext"/> مشترک و
/// مرز تراکنش دیتابیس گرد هم می‌آورد تا سرویس‌های برنامه بتوانند چند repository را
/// به‌صورت اتمیک هماهنگ کنند. به‌صورت scoped ثبت می‌شود؛ نمونه‌ی کانتکست همانی است که
/// همه‌ی repository های همین scope از آن استفاده می‌کنند.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly OrderManagementDbContext _context;
    private readonly ICustomerRepository _customers;
    private readonly ICategoryRepository _categories;
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly IOrderItemRepository _orderItems;
    private readonly IRepository<OrderStatusHistory, Guid> _orderStatusHistory;
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;

    private IDbContextTransaction? _transaction;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="UnitOfWork"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
    /// <param name="customers">repository مشتریان.</param>
    /// <param name="categories">repository دسته‌بندی‌ها.</param>
    /// <param name="products">repository محصولات.</param>
    /// <param name="orders">repository سفارش‌ها.</param>
    /// <param name="orderItems">repository ردیف‌های سفارش.</param>
    /// <param name="orderStatusHistory">repository عمومی تاریخچه‌ی وضعیت سفارش‌ها.</param>
    /// <param name="users">repository کاربران.</param>
    /// <param name="refreshTokens">repository توکن‌های refresh.</param>
    /// <exception cref="ArgumentNullException">وقتی هر آرگومانی برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public UnitOfWork(
        OrderManagementDbContext context,
        ICustomerRepository customers,
        ICategoryRepository categories,
        IProductRepository products,
        IOrderRepository orders,
        IOrderItemRepository orderItems,
        IRepository<OrderStatusHistory, Guid> orderStatusHistory,
        IUserRepository users,
        IRefreshTokenRepository refreshTokens)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _customers = customers ?? throw new ArgumentNullException(nameof(customers));
        _categories = categories ?? throw new ArgumentNullException(nameof(categories));
        _products = products ?? throw new ArgumentNullException(nameof(products));
        _orders = orders ?? throw new ArgumentNullException(nameof(orders));
        _orderItems = orderItems ?? throw new ArgumentNullException(nameof(orderItems));
        _orderStatusHistory = orderStatusHistory ?? throw new ArgumentNullException(nameof(orderStatusHistory));
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _refreshTokens = refreshTokens ?? throw new ArgumentNullException(nameof(refreshTokens));
    }

    /// <inheritdoc />
    public ICustomerRepository Customers => _customers;

    /// <inheritdoc />
    public ICategoryRepository Categories => _categories;

    /// <inheritdoc />
    public IProductRepository Products => _products;

    /// <inheritdoc />
    public IOrderRepository Orders => _orders;

    /// <inheritdoc />
    public IOrderItemRepository OrderItems => _orderItems;

    /// <inheritdoc />
    public IRepository<OrderStatusHistory, Guid> OrderStatusHistory => _orderStatusHistory;

    /// <inheritdoc />
    public IUserRepository Users => _users;

    /// <inheritdoc />
    public IRefreshTokenRepository RefreshTokens => _refreshTokens;

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">وقتی تراکنشی از قبل فعال باشد پرتاب می‌شود.</exception>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            throw new InvalidOperationException("یک تراکنش دیتابیس از قبل فعال است.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">وقتی تراکنش فعالی وجود نداشته باشد پرتاب می‌شود.</exception>
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("تراکنش دیتابیس فعالی برای commit وجود ندارد.");
        }

        IDbContextTransaction transaction = _transaction;
        _transaction = null;

        try
        {
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            // Dispose کردن تراکنشی که commit آن ناموفق بوده، آن را روی سرور rollback می‌کند.
            await transaction.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        // عمداً وقتی تراکنش فعالی وجود ندارد هیچ کاری نمی‌کند: فراخوانندگان از داخل
        // یک بلوک catch rollback می‌کنند و پرتاب کردن در اینجا خطای اصلی را می‌پوشاند
        // (برای نمونه وقتی خودِ commit تراکنش را dispose کرده است).
        if (_transaction is null)
        {
            return;
        }

        IDbContextTransaction transaction = _transaction;
        _transaction = null;

        await transaction.RollbackAsync(cancellationToken);
        await transaction.DisposeAsync();
    }

    /// <summary>
    /// تراکنش فعال (در صورت وجود) و کانتکست دیتابیس زیرین را dispose می‌کند.
    /// کانتکست متعلق به همین scope از unit of work است؛ دوبار dispose کردن آن بی‌ضرر است.
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
