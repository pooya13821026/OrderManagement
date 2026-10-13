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
}
