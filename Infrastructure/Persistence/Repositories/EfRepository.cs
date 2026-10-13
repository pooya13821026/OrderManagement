using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// repository عمومی EF Core که عملیات CRUD پایه‌ی <see cref="IRepository{TEntity,TKey}"/>
/// را روی <see cref="OrderManagementDbContext"/> مشترک و scoped فراهم می‌کند.
/// جست‌وجوهای کلیدی (<see cref="GetByIdAsync"/>) موجودیت‌های <b>tracked</b> برمی‌گردانند، چون
/// سرویس‌های برنامه آن‌ها را تغییر و ذخیره می‌کنند و چون ممکن است یک سرویس گراف جدیدی از
/// موجودیت‌ها را متصل کند که به آن‌ها ارجاع می‌دهد (یک سفارش به مشتری و محصولات از قبل
/// بارگذاری‌شده‌اش ارجاع می‌دهد - یک ارجاع detached دوباره درج می‌شود). پرس‌وجوهای
/// فهرست/جست‌وجو که فقط کد نمایش را تغییر می‌دهند، توسط repository های مشتق‌شده با
/// <c>AsNoTracking</c> پیاده می‌شوند.
/// </summary>
/// <typeparam name="TEntity">نوع موجودیت.</typeparam>
/// <typeparam name="TKey">نوع کلید اصلی.</typeparam>
public abstract class EfRepository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
    where TKey : notnull
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="EfRepository{TEntity,TKey}"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="context"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    protected EfRepository(OrderManagementDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// کانتکست دیتابیسی که همه‌ی repository های همین scope آن را به اشتراک می‌گذارند.
    /// </summary>
    protected OrderManagementDbContext Context { get; }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(id);

        // عمداً tracked (FindAsync): فراخوانندگان یا نتیجه را تغییر و Update() می‌کنند یا
        // از آن در گراف جدیدی از موجودیت‌ها استفاده می‌کنند که به Add() سپرده می‌شود.
        return await Context.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // فهرست فقط‌خواندنی برای نمایش: بدون هزینه‌ی ردیابی تغییرات.
        return await Context.Set<TEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await Context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
    }

    /// <inheritdoc />
    public virtual void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Context.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc />
    public virtual void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        Context.Set<TEntity>().Remove(entity);
    }

    /// <inheritdoc />
    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        Context.Set<TEntity>().RemoveRange(entities);
    }
}
