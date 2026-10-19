using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// پیاده‌سازی EF Core برای <see cref="IOrderRepository"/>: بارگذاری جزئیات با include ها،
/// بررسی‌های وجود و پرس‌وجوی فهرست فیلتر/مرتب/صفحه‌بندی‌شده. پرس‌وجوهای نمایشی از
/// <c>AsNoTracking</c> استفاده می‌کنند.
/// </summary>
public class OrderRepository : EfRepository<Order, Guid>, IOrderRepository
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="OrderRepository"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
    public OrderRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Order?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // عمداً tracked: این جست‌وجو علاوه بر نمای جزئیات، گذار وضعیت را هم تغذیه می‌کند
        // که گراف برگردانده‌شده را تغییر و ذخیره می‌کند - بارگذاری tracked آن، تفکیک
        // هویت (identity resolution) را سالم نگه می‌دارد به‌جای اینکه نمونه‌ی دومی متصل شود.
        return await Context.Orders
            .Include(order => order.Customer)
            .Include(order => order.Items)
                .ThenInclude(item => item.Product)
            .Include(order => order.StatusHistory)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return null;
        }

        return await Context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(order => order.OrderNumber == orderNumber, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedEntities<Order>> GetPagedAsync(
        Guid? customerId,
        OrderStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm,
        string? sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Customer include می‌شود چون نگاشت خروجی از نام مشتری استفاده می‌کند؛
        // بدون آن CustomerName همیشه خالی می‌ماند.
        IQueryable<Order> query = Context.Orders
            .AsNoTracking()
            .Include(order => order.Customer);

        if (customerId.HasValue)
        {
            query = query.Where(order => order.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        // مرزهای شمولی: سفارشی که دقیقاً در fromDate/toDate ثبت شده باشد شامل می‌شود.
        if (fromDate.HasValue)
        {
            query = query.Where(order => order.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            // مرزی بدون مؤلفه‌ی زمان (یک تاریخ خالص مانند 2026-10-01) کل روز را پوشش
            // می‌دهد، بنابراین «تا 2026-10-01» سفارش‌های ثبت‌شده در همان روز را شامل می‌شود.
            // زمان‌نشان‌های دارای مؤلفه‌ی زمان دقیقاً همان‌طور که داده شده‌اند اعمال می‌شوند.
            // استثنا: خودِ MaxValue.Date دیگر روزِ بعدی ندارد و AddDays(1) پرتاب می‌کند؛
            // چون هیچ چیز بعد از آن نمی‌آید، شرط کران بالا اصلاً اضافه نمی‌شود.
            DateTime to = toDate.Value;
            if (to != DateTime.MaxValue.Date)
            {
                DateTime upperBound = to == to.Date
                    ? to.Date.AddDays(1).AddTicks(-1)
                    : to;
                query = query.Where(order => order.OrderDate <= upperBound);
            }
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim();
            query = query.Where(order =>
                order.OrderNumber.Contains(term)
                || (order.ShippingAddress != null && order.ShippingAddress.Contains(term))
                || (order.Notes != null && order.Notes.Contains(term)));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        query = ApplySort(query, sortBy, sortDescending);

        int safePageNumber = Math.Max(pageNumber, 1);
        int safePageSize = Math.Max(pageSize, 1);

        // ضرب در long محاسبه و سقف int می‌گیرد تا شماره‌ی صفحه‌ی بزرگ سرریز منفی نکند.
        int skip = (int)Math.Min((long)(safePageNumber - 1) * safePageSize, int.MaxValue);

        IReadOnlyList<Order> items = await query
            .Skip(skip)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedEntities<Order>(items, totalCount);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
        {
            return false;
        }

        return await Context.Orders
            .AnyAsync(order => order.OrderNumber == orderNumber, cancellationToken);
    }

    /// <summary>
    /// یک ستون مرتب‌سازی از فهرست سفید را روی پرس‌وجوی سفارش اعمال می‌کند. فقط
    /// <c>orderdate</c>، <c>totalamount</c>، <c>status</c> و <c>ordernumber</c> پذیرفته
    /// می‌شوند؛ هر چیز دیگری (از جمله <c>null</c>) به مرتب‌سازی پیش‌فرض بر اساس تاریخ
    /// سفارش برمی‌گردد، بنابراین مقدار ارسالی کلاینت هرگز نمی‌تواند بر شکل پرس‌وجو اثر بگذارد.
    /// </summary>
    /// <param name="query">پرس‌وجوی فیلترشده.</param>
    /// <param name="sortBy">نام ستون مرتب‌سازی درخواستی.</param>
    /// <param name="descending">آیا به‌جای صعودی نزولی مرتب شود.</param>
    /// <returns>پرس‌وجوی مرتب‌شده.</returns>
    private static IOrderedQueryable<Order> ApplySort(IQueryable<Order> query, string? sortBy, bool descending)
    {
        string key = sortBy?.Trim().ToLowerInvariant() ?? string.Empty;

        return key switch
        {
            "totalamount" => descending
                ? query.OrderByDescending(order => order.TotalAmount)
                : query.OrderBy(order => order.TotalAmount),
            "status" => descending
                ? query.OrderByDescending(order => order.Status)
                : query.OrderBy(order => order.Status),
            "ordernumber" => descending
                ? query.OrderByDescending(order => order.OrderNumber)
                : query.OrderBy(order => order.OrderNumber),
            "orderdate" => descending
                ? query.OrderByDescending(order => order.OrderDate)
                : query.OrderBy(order => order.OrderDate),
            // ستون مرتب‌سازی ناشناخته یا غایب: پیش‌فرض روی تاریخ سفارش.
            _ => descending
                ? query.OrderByDescending(order => order.OrderDate)
                : query.OrderBy(order => order.OrderDate)
        };
    }
}
