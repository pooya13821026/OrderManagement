using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OrderManagement.Infrastructure.Persistence;

/// <summary>
/// کارخانه‌ی design-time که فقط توسط ابزارهای CLI مربوط به EF Core
/// (<c>dotnet ef migrations ...</c>) استفاده می‌شود تا ابزارها بتوانند
/// <see cref="OrderManagementDbContext"/> را بدون اجرای <c>Api/Program.cs</c>
/// (که وب‌هاست، Serilog و seeder را بالا می‌آورد) بسازند.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OrderManagementDbContext>
{
    /// <summary>
    /// کانتکست مربوط به عملیات design-time را با پیکربندی پروژه‌ی API در صورت وجود
    /// می‌سازد و در غیر این صورت به LocalDB برمی‌گردد.
    /// </summary>
    /// <param name="args">آرگومان‌های خط فرمان که توسط ابزارهای EF ارسال شده‌اند.</param>
    /// <returns>یک نمونه‌ی پیکربندی‌شده از کانتکست.</returns>
    public OrderManagementDbContext CreateDbContext(string[] args)
    {
        // مونتاژ کارخانه در Infrastructure/bin است؛ یک سطح بالا می‌رویم تا به ریشه‌ی solution
        // و سپس به Api/ برسیم تا migration ها همان رشته‌ی اتصال زمان اجرا را بردارند.
        string basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Api");

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(basePath) ? basePath : Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=OrderManagementDb;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new OrderManagementDbContext(options);
    }
}
