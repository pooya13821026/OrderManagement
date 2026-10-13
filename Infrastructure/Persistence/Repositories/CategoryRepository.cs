using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// پیاده‌سازی EF Core برای <see cref="ICategoryRepository"/>. همه‌ی پرس‌وجوهایی که کد نمایش
/// را تغذیه می‌کنند بدون ردیابی تغییرات اجرا می‌شوند (<c>AsNoTracking</c>).
/// </summary>
public class CategoryRepository : EfRepository<Category, Guid>, ICategoryRepository
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="CategoryRepository"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
    public CategoryRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        // دفاعی: مقدار جست‌وجوی خالی هرگز نمی‌تواند با نام ذخیره‌شده‌ای مطابقت کند، پس پرس‌وجو را رد می‌کنیم.
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return await Context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Name == name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        return await Context.Categories
            .AnyAsync(category => category.Name == name, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedEntities<Category>> GetPagedAsync(
        string? searchTerm,
        bool? isActive,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = Context.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim();
            query = query.Where(category =>
                category.Name.Contains(term)
                || (category.Description != null && category.Description.Contains(term)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(category => category.IsActive == isActive.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        // ورودی‌های صفحه‌بندی را محدود می‌کنیم تا اشتباه فراخواننده هرگز نتواند Skip/Take منفی تولید کند.
        int safePageNumber = Math.Max(pageNumber, 1);
        int safePageSize = Math.Max(pageSize, 1);

        // مرتب‌سازی قطعی (نام، سپس شناسه یکتا به‌عنوان شکننده‌ی تساوی) صفحات را
        // هنگام صفحه‌بندی پایدار نگه می‌دارد - در غیر این صورت ردیف‌ها ممکن است میان
        // درخواست‌های صفحه ظاهر یا ناپدید شوند.
        IReadOnlyList<Category> items = await query
            .OrderBy(category => category.Name)
            .ThenBy(category => category.Id)
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedEntities<Category>(items, totalCount);
    }
}
