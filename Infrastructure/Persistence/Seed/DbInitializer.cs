using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Security;

namespace OrderManagement.Infrastructure.Persistence.Seed;

/// <summary>
/// داده‌کاشت idempotent دیتابیس: روی یک دیتابیس خالی دو حساب بوت‌استرپ (یکی برای هر نقش)،
/// دقیقاً ۵۰ مشتری و ۲۰۰ محصول را با یک seed قطعی Bogus می‌سازد و در هر اجرای بعدی فقط
/// ردیف‌های غایب را اضافه می‌کند. هیچ migration ای اینجا اعمال نمی‌شود - این مسئولیت
/// فراخواننده است.
/// </summary>
public static class DbInitializer
{
    /// <summary>
    /// تعداد مشتریانی که داده‌کاشت تضمین می‌کند.
    /// </summary>
    public const int CustomerSeedCount = 50;

    /// <summary>
    /// تعداد محصولاتی که داده‌کاشت تضمین می‌کند.
    /// </summary>
    public const int ProductSeedCount = 200;

    /// <summary>
    /// seed ثابت Bogus تا هر اجرا همان داده‌های نمونه را تولید کند.
    /// </summary>
    private const int BogusSeed = 12345;

    /// <summary>آدرس ورود مدیر داده‌کاشت‌شده.</summary>
    private const string AdminEmail = "admin@ordermanagement.com";

    /// <summary>رمز عبور ورود مدیر داده‌کاشت‌شده (داده‌ی seed مربوط به محیط توسعه).</summary>
    private const string AdminPassword = "Admin@123";

    /// <summary>آدرس ورود کاربر عادی داده‌کاشت‌شده.</summary>
    private const string UserEmail = "user@ordermanagement.com";

    /// <summary>رمز عبور ورود کاربر عادی داده‌کاشت‌شده (داده‌ی seed مربوط به محیط توسعه).</summary>
    private const string UserPassword = "User@123";

    /// <summary>
    /// کاربران، مشتریان و محصولات را به‌صورت idempotent داده‌کاشت می‌کند. ردیف‌های موجود
    /// هرگز لمس نمی‌شوند؛ یک دیتابیس نیمه‌کاشت‌شده تا دقیقاً ۵۰ مشتری و ۲۰۰ محصول کامل می‌شود.
    /// </summary>
    /// <param name="context">کانتکست دیتابیسی که باید داده‌کاشت شود.</param>
    /// <param name="logger">لاگری که برای گزارش تعداد نهایی ردیف‌ها استفاده می‌شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="context"/> یا <paramref name="logger"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public static async Task SeedAsync(
        OrderManagementDbContext context,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        int createdUsers = await SeedUsersAsync(context, cancellationToken);
        int createdCustomers = await SeedCustomersAsync(context, cancellationToken);
        int createdProducts = await SeedProductsAsync(context, cancellationToken);

        // یک ذخیره‌ی واحد کل داده‌کاشت را اتمیک نگه می‌دارد: یا همه‌ی ردیف‌ها می‌نشینند یا هیچ‌کدام.
        await context.SaveChangesAsync(cancellationToken);

        int userCount = await context.Users.CountAsync(cancellationToken);
        int customerCount = await context.Customers.CountAsync(cancellationToken);
        int productCount = await context.Products.CountAsync(cancellationToken);

        // نقش‌ها رشته‌های ساده روی ردیف کاربرند (مدل دامنه جدول نقش ندارد)،
        // بنابراین «ایجاد نقش‌ها» یعنی تضمین وجود یک حساب داده‌کاشت‌شده برای هر نقش شناخته‌شده.
        logger.LogInformation(
            "داده‌کاشت پایان یافت: نقش‌ها [{Roles}]; {UserCount} کاربر ({CreatedUsers} ایجاد شد)، " +
            "{CustomerCount} مشتری ({CreatedCustomers} ایجاد شد)، {ProductCount} محصول ({CreatedProducts} ایجاد شد).",
            string.Join(", ", new[] { Roles.Admin, Roles.User }),
            userCount,
            createdUsers,
            customerCount,
            createdCustomers,
            productCount,
            createdProducts);
    }

    /// <summary>
    /// برای هر نقش شناخته‌شده یک حساب می‌سازد (مدیر با <see cref="Roles.Admin"/>،
    /// کاربر عادی با <see cref="Roles.User"/>) مگر اینکه از قبل وجود داشته باشد.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>تعداد کاربرانی که ایجاد شدند.</returns>
    private static async Task<int> SeedUsersAsync(OrderManagementDbContext context, CancellationToken cancellationToken)
    {
        var passwordHasher = new BCryptPasswordHasher();
        int created = 0;

        if (!await context.Users.AnyAsync(user => user.Email == AdminEmail, cancellationToken))
        {
            context.Users.Add(new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = AdminEmail,
                PasswordHash = passwordHasher.Hash(AdminPassword),
                FirstName = "Admin",
                LastName = "Admin",
                IsActive = true,
                Role = Roles.Admin,
                SecurityStamp = Guid.NewGuid().ToString("N")
            });
            created++;
        }

        if (!await context.Users.AnyAsync(user => user.Email == UserEmail, cancellationToken))
        {
            context.Users.Add(new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "user",
                Email = UserEmail,
                PasswordHash = passwordHasher.Hash(UserPassword),
                FirstName = "Demo",
                LastName = "User",
                IsActive = true,
                Role = Roles.User,
                SecurityStamp = Guid.NewGuid().ToString("N")
            });
            created++;
        }

        return created;
    }

    /// <summary>
    /// جدول مشتریان را با یک توالی قطعی Bogus (seed 12345) تا
    /// <see cref="CustomerSeedCount"/> ردیف کامل می‌کند. ایمیل‌ها شماره‌ی ردیف را در خود
    /// دارند، بنابراین حتی وقتی داده‌کاشت ناقص توسط اجرای بعدی کامل شود یکتا می‌مانند.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>تعداد مشتریانی که ایجاد شدند.</returns>
    private static async Task<int> SeedCustomersAsync(OrderManagementDbContext context, CancellationToken cancellationToken)
    {
        int existingCount = await context.Customers.CountAsync(cancellationToken);
        int missingCount = CustomerSeedCount - existingCount;
        if (missingCount <= 0)
        {
            return 0;
        }

        var customerFaker = new Faker<Customer>("en")
            .UseSeed(BogusSeed)
            .RuleFor(customer => customer.FirstName, faker => faker.Name.FirstName())
            .RuleFor(customer => customer.LastName, faker => faker.Name.LastName())
            .RuleFor(customer => customer.Phone, faker => faker.Phone.PhoneNumber())
            .RuleFor(customer => customer.Address, faker => faker.Address.FullAddress());

        for (int index = existingCount; index < CustomerSeedCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Customer customer = customerFaker.Generate();
            customer.Id = Guid.NewGuid();
            // شماره‌ی ردیف، آدرس‌های تولیدشده را در میان همه‌ی اجراها یکتا نگه می‌دارد.
            customer.Email = BuildEmail(customer.FirstName, customer.LastName, index + 1);
            context.Customers.Add(customer);
        }

        return missingCount;
    }

    /// <summary>
    /// جدول محصولات را با یک توالی قطعی Bogus (seed 12345) تا
    /// <see cref="ProductSeedCount"/> ردیف کامل می‌کند. SKU ها از قالب یکتای
    /// <c>SKU-0001</c> پیروی می‌کنند و هر دهمین محصول (دقیقاً ۱۰٪) غیرفعال است.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>تعداد محصولاتی که ایجاد شدند.</returns>
    private static async Task<int> SeedProductsAsync(OrderManagementDbContext context, CancellationToken cancellationToken)
    {
        int existingCount = await context.Products.CountAsync(cancellationToken);
        int missingCount = ProductSeedCount - existingCount;
        if (missingCount <= 0)
        {
            return 0;
        }

        var productFaker = new Faker<Product>("en")
            .UseSeed(BogusSeed)
            .RuleFor(product => product.Name, faker => faker.Commerce.ProductName())
            .RuleFor(product => product.Description, faker => faker.Commerce.ProductDescription())
            .RuleFor(product => product.Price, faker => Math.Round(faker.Random.Decimal(1m, 1000m), 2))
            .RuleFor(product => product.StockQuantity, faker => faker.Random.Int(0, 500));

        for (int index = existingCount; index < ProductSeedCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Product product = productFaker.Generate();
            product.Id = Guid.NewGuid();
            int rowNumber = index + 1;
            product.Sku = $"SKU-{rowNumber:D4}";
            // سهم قطعی ۱۰٪ غیرفعال: هر دهمین محصول بازنشسته می‌شود.
            product.IsActive = rowNumber % 10 != 0;
            context.Products.Add(product);
        }

        return missingCount;
    }

    /// <summary>
    /// یک آدرس نمونه‌ی یکتا و امن برای صندوق ایمیل از روی یک نام و شماره‌ی ردیف می‌سازد،
    /// برای نمونه <c>john.smith.001@example.com</c>.
    /// </summary>
    /// <param name="firstName">نام.</param>
    /// <param name="lastName">نام خانوادگی.</param>
    /// <param name="rowNumber">شماره‌ی ردیف که از ۱ شروع می‌شود.</param>
    /// <returns>آدرس ایمیل تولیدشده.</returns>
    private static string BuildEmail(string firstName, string lastName, int rowNumber)
    {
        string localPart = SanitizeEmailPart($"{firstName}.{lastName}");
        return $"{localPart}.{rowNumber:D3}@example.com";
    }

    /// <summary>
    /// مقدار را به حروف کوچک تبدیل می‌کند و فقط حروف، ارقام و نقطه‌های ASCII را نگه می‌دارد،
    /// بنابراین نام‌های تولیدشده (برای نمونه <c>O'Brien</c>) هرگز نمی‌توانند آدرس نامعتبر
    /// بسازند.
    /// </summary>
    /// <param name="value">مقدار خام.</param>
    /// <returns>مقدار پاک‌سازی‌شده، یا <c>customer</c> وقتی چیز قابل استفاده‌ای باقی نمانده باشد.</returns>
    private static string SanitizeEmailPart(string value)
    {
        string lowered = value.ToLowerInvariant();
        char[] cleaned = lowered
            .Where(character =>
                character is >= 'a' and <= 'z'
                or >= '0' and <= '9'
                or '.')
            .ToArray();

        string result = new(cleaned);
        return string.IsNullOrEmpty(result) ? "customer" : result;
    }
}
