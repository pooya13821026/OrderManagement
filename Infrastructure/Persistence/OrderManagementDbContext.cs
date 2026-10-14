using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// کانتکست دیتابیس Entity Framework Core برای ذخیره‌سازی مدیریت سفارش‌ها.
/// موجودیت‌های دامنه را روی جدول‌های SQL Server نگاشت می‌کند، همه‌ی
/// <c>IEntityTypeConfiguration&lt;T&gt;</c> اعلام‌شده در این مونتاژ را اعمال می‌کند و
/// پیش از هر ذخیره، زمان‌نشان‌های ایجاد/ویرایش UTC را ثبت می‌کند.
/// </summary>
public class OrderManagementDbContext : DbContext
{
    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="OrderManagementDbContext"/> می‌سازد.
    /// </summary>
    /// <param name="options">گزینه‌های کانتکست (provider و رشته‌ی اتصال).</param>
    public OrderManagementDbContext(DbContextOptions<OrderManagementDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// مشتریان فروشگاه را دریافت می‌کند.
    /// </summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>
    /// دسته‌بندی‌های کاتالوگ را دریافت می‌کند.
    /// </summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// محصولات کاتالوگ را دریافت می‌کند.
    /// </summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>
    /// سفارش‌های مشتریان را دریافت می‌کند.
    /// </summary>
    public DbSet<Order> Orders => Set<Order>();

    /// <summary>
    /// ردیف‌های سفارش را دریافت می‌کند.
    /// </summary>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// ردیف‌های تاریخچه‌ی تغییر وضعیت سفارش‌ها را دریافت می‌کند.
    /// </summary>
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    /// <summary>
    /// حساب‌های کاربری برنامه که برای احراز هویت استفاده می‌شوند را دریافت می‌کند.
    /// </summary>
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    /// <summary>
    /// هش‌های ذخیره‌شده‌ی توکن refresh را دریافت می‌کند.
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// مدل را با اعمال قراردادهای پایه و سپس همه‌ی پیکربندی‌های نوع موجودیت
    /// از این مونتاژ می‌سازد، بنابراین همه‌ی نگاشت‌ها کنار کد پایداری می‌ماند.
    /// </summary>
    /// <param name="modelBuilder">سازنده‌ای که برای پیکربندی مدل استفاده می‌شود.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderManagementDbContext).Assembly);
    }

    /// <summary>
    /// <c>CreatedAt</c>/<c>UpdatedAt</c> را با زمان فعلی UTC ثبت می‌کند و سپس
    /// همه‌ی تغییرات معلق را ذخیره می‌کند. موجودیت‌هایی که از قبل زمان‌نشان صریح دارند
    /// آن را نگه می‌دارند؛ هر موجودیت تغییریافته <c>UpdatedAt</c> خود را تازه می‌کند.
    /// </summary>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>تعداد ورودی‌های وضعیتی که در ذخیره‌سازی نوشته شده‌اند.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyUtcTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// change tracker را طی می‌کند و زمان‌نشان‌های UTC را اختصاص می‌دهد:
    /// موجودیت‌های افزوده‌شده وقتی مقدار تنظیم نشده باشد <c>CreatedAt</c> (و <c>UpdatedAt</c>)
    /// می‌گیرند، موجودیت‌های تغییریافته <c>UpdatedAt</c> تازه می‌گیرند. موجودیت‌های بدون چنین
    /// پروپرتی‌ای (برای نمونه <see cref="RefreshToken"/>) نادیده گرفته می‌شوند.
    /// </summary>
    private void ApplyUtcTimestamps()
    {
        DateTime now = DateTime.UtcNow;

        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                SetTimestampIfPresent(entry, "CreatedAt", now, onlyWhenUnset: true);
                SetTimestampIfPresent(entry, "UpdatedAt", now, onlyWhenUnset: true);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetTimestampIfPresent(entry, "UpdatedAt", now, onlyWhenUnset: false);
            }
        }
    }

    /// <summary>
    /// <paramref name="value"/> را به پروپرتی زمان‌نشان داده‌شده وقتی نوع موجودیت آن را
    /// اعلام کرده باشد اختصاص می‌دهد، و به‌صورت اختیاری فقط وقتی مقدار فعلی هنوز مقدار
    /// پیش‌فرض نوع است.
    /// </summary>
    /// <param name="entry">موجودیت ردیابی‌شده.</param>
    /// <param name="propertyName">نام پروپرتی زمان‌نشان.</param>
    /// <param name="value">زمان‌نشان UTC که باید اختصاص یابد.</param>
    /// <param name="onlyWhenUnset">آیا مقدار از قبل تنظیم‌شده باید حفظ شود.</param>
    private static void SetTimestampIfPresent(EntityEntry entry, string propertyName, DateTime value, bool onlyWhenUnset)
    {
        // هر موجودیتی هر دو زمان‌نشان را ندارد (کاربران و توکن‌ها UpdatedAt ندارند).
        if (entry.Metadata.FindProperty(propertyName) is null)
        {
            return;
        }

        if (onlyWhenUnset
            && entry.Property(propertyName).CurrentValue is DateTime current
            && current != default)
        {
            return;
        }

        entry.Property(propertyName).CurrentValue = value;
    }
}
