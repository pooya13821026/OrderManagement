using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// <see cref="RefreshToken"/> را روی جدول <c>RefreshTokens</c> نگاشت می‌کند: فقط هش توکن
/// ذخیره می‌شود (یکتا)، توکن‌ها همراه با کاربرشان حذف می‌شوند و پرچم محاسباتی فعال
/// هرگز ذخیره نمی‌شود.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);

        builder.Property(token => token.UserId).IsRequired();
        // دیجست hex مربوط به SHA256 چیزی است که ذخیره و جست‌وجو می‌شود؛ توکن خام
        // دقیقاً یک بار به کلاینت بازگردانده می‌شود و هرگز ذخیره نمی‌شود.
        builder.Property(token => token.TokenHash).IsRequired().HasMaxLength(128);
        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.Property(token => token.CreatedAt).IsRequired();
        builder.Property(token => token.ReplacedByTokenHash).HasMaxLength(128);

        // جست‌وجو بر اساس هش، refresh و لغو (revoke) را ممکن می‌کند و هیچ دو توکنی نمی‌توانند هش مشترک داشته باشند.
        builder.HasIndex(token => token.TokenHash).IsUnique();

        // IsActive از RevokedAt/ExpiresAt مشتق می‌شود؛ ذخیره‌ی آن باعث انحراف می‌شد.
        builder.Ignore(token => token.IsActive);

        // توکن‌های refresh بدون حساب‌شان بی‌معنایند: آن‌ها را با هم حذف کنید.
        builder.HasOne(token => token.User)
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
