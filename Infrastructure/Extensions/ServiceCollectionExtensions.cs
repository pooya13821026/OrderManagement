using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Persistence.Repositories;
using OrderManagement.Infrastructure.Security;
using OrderManagement.Infrastructure.Services;

namespace OrderManagement.Infrastructure.Extensions;

/// <summary>
/// نقاط ثبت DI برای لایه‌ی Infrastructure.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// پایداری SQL Server به‌علاوه‌ی همه‌ی repository ها، unit of work، سرویس‌های
    /// امنیتی (هش‌کننده‌ی رمز عبور، سرویس توکن JWT) و سرویس اطلاع‌رسانی وضعیت سفارش
    /// را ثبت می‌کند. همه‌ی موارد scoped هستند،
    /// بنابراین همه‌ی repository های یک درخواست یک <see cref="OrderManagementDbContext"/> مشترک دارند.
    /// </summary>
    /// <param name="services">مجموعه‌ی سرویس‌ها.</param>
    /// <param name="configuration">پیکربندی برنامه؛ <c>ConnectionStrings:DefaultConnection</c> را می‌خواند.</param>
    /// <returns>مجموعه‌ی سرویس‌ها، برای زنجیره‌سازی.</returns>
    /// <exception cref="ArgumentNullException">وقتی یک آرگومان برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    /// <exception cref="InvalidOperationException">وقتی رشته‌ی اتصال پیش‌فرض غایب باشد پرتاب می‌شود.</exception>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "رشته‌ی اتصال 'ConnectionStrings:DefaultConnection' وجود ندارد.");

        services.AddDbContext<OrderManagementDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderItemRepository, OrderItemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // تاریخچه‌ی وضعیت سفارش از قرارداد عمومی مخزن استفاده می‌کند؛ پیاده‌سازی
        // نازک از EfRepository است چون نوع پایه انتزاعی است و DI آن را نمی‌سازد.
        services.AddScoped<IRepository<OrderStatusHistory, Guid>, OrderStatusHistoryRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // اطلاع‌رسانی گذار وضعیت سفارش: در حال حاضر فقط لاگ ساخت‌یافته (بدون SMS/ایمیل).
        services.AddScoped<INotificationService, LogNotificationService>();

        return services;
    }
}
