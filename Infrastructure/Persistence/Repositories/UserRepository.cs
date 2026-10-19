using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// پیاده‌سازی EF Core برای <see cref="IUserRepository"/>. جست‌وجوی ایمیل جریان ورود به
/// سیستم را تغذیه می‌کند (که کاربر را به‌روزرسانی و ذخیره می‌کند) و به همین دلیل موجودیت‌های
/// tracked برمی‌گرداند؛ جست‌وجوی نام کاربری فقط برای نمایش است و با <c>AsNoTracking</c> اجرا می‌شود.
/// </summary>
public class UserRepository : EfRepository<ApplicationUser, Guid>, IUserRepository
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="UserRepository"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
    public UserRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        // عمداً tracked: جریان ورود LastLoginAt را روی کاربر برگردانده‌شده به‌روزرسانی و
        // ذخیره می‌کند؛ یک نسخه‌ی detached با نمونه‌ی از قبل ردیابی‌شده تداخل پیدا می‌کند.
        return await Context.Users
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ApplicationUser?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        return await Context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.UserName == userName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        return await Context.Users
            .AnyAsync(user => user.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedEntities<ApplicationUser>> GetPagedAsync(
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // فهرست مدیریت فقط‌خواندنی است (کنترلر Admin)؛ بدون ردیابی تغذیه می‌شود.
        IQueryable<ApplicationUser> query = Context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            string term = searchTerm.Trim();
            query = query.Where(user =>
                user.UserName.Contains(term)
                || user.Email.Contains(term)
                || user.FirstName.Contains(term)
                || user.LastName.Contains(term));
        }

        int totalCount = await query.CountAsync(cancellationToken);

        // ورودی‌های صفحه‌بندی را محدود می‌کنیم تا اشتباه فراخواننده هرگز نتواند Skip/Take منفی تولید کند.
        int safePageNumber = Math.Max(pageNumber, 1);
        int safePageSize = Math.Max(pageSize, 1);

        // مرتب‌سازی قطعی (نام کاربری، سپس ایمیل یکتا به‌عنوان شکننده‌ی تساوی) صفحات را
        // هنگام صفحه‌بندی پایدار نگه می‌دارد - در غیر این صورت ردیف‌ها ممکن است میان
        // درخواست‌های صفحه ظاهر یا ناپدید شوند.
        // ضرب در long محاسبه و سقف int می‌گیرد تا شماره‌ی صفحه‌ی بزرگ سرریز منفی نکند.
        int skip = (int)Math.Min((long)(safePageNumber - 1) * safePageSize, int.MaxValue);

        IReadOnlyList<ApplicationUser> items = await query
            .OrderBy(user => user.UserName)
            .ThenBy(user => user.Email)
            .Skip(skip)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedEntities<ApplicationUser>(items, totalCount);
    }
}
