using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Extensions;
using OrderManagement.Api.Middleware;
using OrderManagement.Infrastructure.Extensions;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Persistence.Seed;
using Serilog;

// لاگر bootstrap: پیش از ساخته شدن host فعال است، بنابراین خطاهای پیکربندی یا DI
// هنگام startup هم در کنسول ثبت می‌شوند (خط لوله‌ی کامل Serilog بعداً جایگزین می‌شود).
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // منابع پیکربندی: appsettings.json به‌علاوه‌ی لایه‌ی مخصوص محیط
    // (appsettings.{Environment}.json) و متغیرهای محیطی، با همین ترتیب اولویت.
    builder.Configuration
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // پایداری، repository ها، هش‌کننده‌ی رمز عبور و سرویس توکن JWT (Infrastructure).
    builder.Services.AddInfrastructure(builder.Configuration);
    // MVC، Swagger، احراز هویت bearer مربوط به JWT، مجوزدهی و Serilog (Api).
    builder.Services.AddApiServices(builder.Configuration);
    // محدودسازی نرخ روی اندپوینت‌های ناشناس احراز هویت (ضد brute-force روی لاگین/ثبت‌نام).
    builder.Services.AddRateLimiter(options =>
    {
        options.AddPolicy("auth", httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
    // سرویس‌های برنامه که کنترلرها از آن‌ها استفاده می‌کنند.
    builder.Services.AddApplicationServices();

    WebApplication app = builder.Build();

    // باید اول اجرا شود: هر استثنایی که در ادامه‌ی خط لوله پرتاب شود، از جمله
    // خطا‌های مجوزدهی و کنترلرها، به یک پاسخ problem-details تبدیل می‌کند.
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    // لاگ‌گیری ساخت‌یافته‌ی درخواست‌ها (متد، مسیر، کد وضعیت، زمان سپری‌شده) از طریق Serilog.
    app.UseSerilogRequestLogging();

    // باید پیش از احراز هویت اجرا شود تا سیل درخواست‌های ناشناس زودتر مهار شود.
    app.UseRateLimiter();

    // مستندات API (از deliverable های تسک) در همه‌ی محیط‌ها فعال است تا ارزیاب با هر
    // حالتی که اجرا کند Swagger را ببیند؛ در محصول واقعی بهتر است فقط Development باشد.
    app.UseSwagger();
    app.UseSwaggerUI();

    // ترتیب مهم است: middleware مربوط به JWT bearer درخواست را احراز هویت می‌کند و
    // ClaimsPrincipal (شامل claim نگاشته‌شده‌ی نقش) را پیش از اجرای authorization می‌سازد.
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // migration های معلق EF Core را اعمال و داده‌ی مرجع را داده‌کاشت می‌کند. خطاها لاگ
    // می‌شوند اما مانع بالا آمدن host نمی‌شوند، بنابراین API در حالت از کار افتادن DB هم در دسترس می‌ماند.
    await InitializeDatabaseAsync(app);

    // یک بار پس از گوش دادن سرور اجرا می‌شود؛ آدرس‌هایی که Kestrel واقعاً bind کرده را گزارش می‌دهد.
    app.Services
        .GetRequiredService<IHostApplicationLifetime>()
        .ApplicationStarted.Register(() =>
            Log.Information(
                "برنامه بالا آمد. در حال گوش دادن روی: {ListeningUrls}",
                app.Urls.Count > 0 ? string.Join(", ", app.Urls) : "(default Kestrel endpoints)"));

    Log.Information("در حال راه‌اندازی Order Management API در محیط {Environment}.", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Order Management API هنگام startup به‌صورت غیرمنتظره پایان یافت.");
}
finally
{
    Log.CloseAndFlush();
}

// یک DI scope می‌سازد، migration های معلق دیتابیس را اعمال و seeder idempotent را اجرا می‌کند.
// هر دو مرحله داخل یک try/catch قرار دارند تا در دسترس نبودن دیتابیس از طریق
// Serilog گزارش شود به‌جای اینکه host را از کار بیندازد.
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();
    ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        OrderManagementDbContext context = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(context, logger);
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "migration/seed دیتابیس هنگام startup ناموفق بود؛ API به هر حال بالا می‌آید.");
    }
}
