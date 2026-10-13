using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// پیاده‌سازی EF Core برای <see cref="IRefreshTokenRepository"/>. خواندن‌های نمایشی ردیابی
/// نمی‌شوند، جست‌وجوی هش توکن refresh موجودیت tracked برمی‌گرداند چون جریان refresh آن را
/// لغو (revoke) می‌کند، و لغو گروهی مستقیماً در دیتابیس اجرا می‌شود و به ذخیره‌ی بعدی نیاز ندارد.
/// </summary>
public class RefreshTokenRepository : EfRepository<RefreshToken, Guid>, IRefreshTokenRepository
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="RefreshTokenRepository"/> می‌سازد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس مشترک scope جاری.</param>
    public RefreshTokenRepository(OrderManagementDbContext context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return null;
        }

        // عمداً tracked: جریان refresh توکن ارائه‌شده را لغو (Update) و ذخیره می‌کند؛ یک
        // نسخه‌ی detached با نمونه‌ی از قبل ردیابی‌شده تداخل پیدا می‌کند.
        // سرویس احراز هویت مالک را dereference می‌کند، پس کاربر هم include می‌شود.
        return await Context.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        DateTime now = DateTime.UtcNow;

        List<RefreshToken> tokens = await Context.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId
                && token.RevokedAt == null
                && token.ExpiresAt > now)
            .OrderByDescending(token => token.CreatedAt)
            .ToListAsync(cancellationToken);

        return tokens;
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // یک UPDATE مبتنی بر set به‌جای بارگذاری همه‌ی توکن‌ها: لغو یک تغییر وضعیت گروهی
        // است که باید اعمال شود حتی وقتی فراخواننده هرگز SaveChanges را صدا نزند، و همچنان
        // داخل تراکنشی که از طریق UnitOfWork باز شده اجرا می‌شود (همان connection).
        await Context.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.RevokedAt, DateTime.UtcNow),
                cancellationToken);
    }
}
