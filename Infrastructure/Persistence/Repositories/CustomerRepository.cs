using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// پیاده‌سازی EF Core برای <see cref="ICustomerRepository"/>. همه‌ی پرس‌وجوهایی که کد نمایش
/// را تغذیه می‌کنند بدون ردیابی تغییرات اجرا می‌شوند (<c>AsNoTracking</c>).
/// </summary>
public class CustomerRepository : EfRepository<Customer, Guid>, ICustomerRepository
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="CustomerRepository"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
    public CustomerRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // دفاعی: مقدار جست‌وجوی خالی هرگز نمی‌تواند با آدرس ذخیره‌شده‌ای مطابقت کند، پس پرس‌وجو را رد می‌کنیم.
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return await Context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Customer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return null;
        }

        // عمداً tracked (برخلاف GetByEmailAsync که فقط‌نمایشی و AsNoTracking است):
        // CreateAsync سفارش، مشتریِ برگردانده‌شده را در گراف سفارش attach می‌کند و یک
        // نمونه‌ی detached دوباره درج می‌شود (همان قاعده‌ی مستندشده در EfRepository).
        return await Context.Customers
            .FirstOrDefaultAsync(customer => customer.UserId == userId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return await Context.Customers
            .AnyAsync(customer => customer.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedEntities<Customer>> GetPagedAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Customer> query = Context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim();
            query = query.Where(customer =>
                customer.FirstName.Contains(term)
                || customer.LastName.Contains(term)
                || customer.Email.Contains(term));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        // ورودی‌های صفحه‌بندی را محدود می‌کنیم تا اشتباه فراخواننده هرگز نتواند Skip/Take منفی تولید کند.
        int safePageNumber = Math.Max(pageNumber, 1);
        int safePageSize = Math.Max(pageSize, 1);

        // مرتب‌سازی قطعی (نام خانوادگی، سپس ایمیل یکتا به‌عنوان شکننده‌ی تساوی) صفحات را
        // هنگام صفحه‌بندی پایدار نگه می‌دارد - در غیر این صورت ردیف‌ها ممکن است میان
        // درخواست‌های صفحه ظاهر یا ناپدید شوند.
        IReadOnlyList<Customer> items = await query
            .OrderBy(customer => customer.LastName)
            .ThenBy(customer => customer.FirstName)
            .ThenBy(customer => customer.Email)
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedEntities<Customer>(items, totalCount);
    }
}
