using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Security;

namespace OrderManagement.Infrastructure.Persistence.Seed;

/// <summary>
/// داده‌کاشت idempotent دیتابیس: روی یک دیتابیس خالی دو حساب بوت‌استرپ (یکی برای هر نقش)،
/// دقیقاً ۵۰ مشتری و ۲۰۰ محصول را با یک seed قطعی Bogus و دسته‌های ثابت کاتالوگ می‌سازد
/// و در هر اجرای بعدی فقط ردیف‌های غایب را اضافه می‌کند. هیچ migration ای اینجا اعمال
/// نمی‌شود - این مسئولیت فراخواننده است.
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
    /// کاربران، مشتریان، دسته‌ها و محصولات را به‌صورت idempotent داده‌کاشت می‌کند. ردیف‌های موجود
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
        (int createdCategories, Dictionary<string, Guid> categoryIdsByName) =
            await SeedCategoriesAsync(context, cancellationToken);
        int createdProducts = await SeedProductsAsync(context, categoryIdsByName, cancellationToken);

        // یک ذخیره‌ی واحد کل داده‌کاشت را اتمیک نگه می‌دارد: یا همه‌ی ردیف‌ها می‌نشینند یا هیچ‌کدام.
        await context.SaveChangesAsync(cancellationToken);

        int userCount = await context.Users.CountAsync(cancellationToken);
        int customerCount = await context.Customers.CountAsync(cancellationToken);
        int categoryCount = await context.Categories.CountAsync(cancellationToken);
        int productCount = await context.Products.CountAsync(cancellationToken);

        // نقش‌ها رشته‌های ساده روی ردیف کاربرند (مدل دامنه جدول نقش ندارد)،
        // بنابراین «ایجاد نقش‌ها» یعنی تضمین وجود یک حساب داده‌کاشت‌شده برای هر نقش شناخته‌شده.
        logger.LogInformation(
            "داده‌کاشت پایان یافت: نقش‌ها [{Roles}]; {UserCount} کاربر ({CreatedUsers} ایجاد شد)، " +
            "{CustomerCount} مشتری ({CreatedCustomers} ایجاد شد)، {CategoryCount} دسته ({CreatedCategories} ایجاد شد)، " +
            "{ProductCount} محصول ({CreatedProducts} ایجاد شد).",
            string.Join(", ", new[] { Roles.Admin, Roles.User }),
            userCount,
            createdUsers,
            customerCount,
            createdCustomers,
            categoryCount,
            createdCategories,
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
                FirstName = "مدیر",
                LastName = "سیستم",
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
                FirstName = "کاربر",
                LastName = "نمایشی",
                IsActive = true,
                Role = Roles.User,
                SecurityStamp = Guid.NewGuid().ToString("N")
            });
            created++;
        }

        return created;
    }

    /// <summary>
    /// جدول مشتریان را با داده‌ی فارسی (نام و نام‌خانوادگی ایرانی، موبایل ایرانی و آدرس
    /// فارسی) تا <see cref="CustomerSeedCount"/> ردیف کامل می‌کند. ایمیل‌ها لاتین و یکتا
    /// می‌مانند چون ایمیل فارسی در دنیای واقعی هم کاربردی ندارد. توالی قطعی است (seed 12345).
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

        // نام‌ها از دیتاست فارسی Bogus؛ تلفن و آدرس با الگوی ایرانی ساخته می‌شوند تا
        // مستقل از کامل بودن دیتاست لوکیال، همیشه شکل واقعی داشته باشند.
        string[] cities = ["تهران", "اصفهان", "شیراز", "مشهد", "تبریز", "کرج", "اهواز", "قم"];
        string[] streets = ["ولیعصر", "انقلاب", "آزادی", "شریعتی", "مطهری", "فردوسی", "حافظ", "سعدی"];

        var customerFaker = new Faker<Customer>("fa")
            .UseSeed(BogusSeed)
            .RuleFor(customer => customer.FirstName, faker => faker.Name.FirstName())
            .RuleFor(customer => customer.LastName, faker => faker.Name.LastName())
            .RuleFor(customer => customer.Phone, faker =>
                $"09{faker.Random.Int(10, 99):D2} {faker.Random.Int(100, 999):D3} {faker.Random.Int(1000, 9999):D4}")
            .RuleFor(customer => customer.Address, faker =>
                $"{faker.PickRandom(cities)}، خیابان {faker.PickRandom(streets)}، پلاک {faker.Random.Int(1, 200)}، واحد {faker.Random.Int(1, 30)}");

        for (int index = existingCount; index < CustomerSeedCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Customer customer = customerFaker.Generate();
            customer.Id = Guid.NewGuid();
            // شماره‌ی ردیف، ایمیل‌ها را در میان همه‌ی اجراها یکتا نگه می‌دارد.
            customer.Email = $"customer.{index + 1:D3}@example.com";
            context.Customers.Add(customer);
        }

        return missingCount;
    }

    /// <summary>
    /// جدول دسته‌بندی‌ها را با نام‌های فارسی کوتاه کاتالوگ (مثلاً کالای دیجیتال، پوشاک، کتاب و…)
    /// پر می‌کند. این روش idempotent است: دسته‌هایی که از قبل با همان نام وجود دارند
    /// دوباره اضافه نمی‌شوند. محصولات موجود عمداً به این دسته‌ها متصل نمی‌شوند تا رفتار
    /// موجود (بدون دسته) حفظ شود.
    /// </summary>
    /// <param name="context">کانتکست دیتابیس.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>تعداد دسته‌های ایجادشده به‌همراه نگاشت نام به شناسه‌ی همه‌ی دسته‌ها
    /// (شامل موجودهای قبلی)، تا محصولات تازه‌سید به دسته‌ی مرتبطشان متصل شوند.</returns>
    private static async Task<(int Created, Dictionary<string, Guid> IdsByName)> SeedCategoriesAsync(
        OrderManagementDbContext context,
        CancellationToken cancellationToken)
    {
        string[] categoryNames =
        [
            "کالای دیجیتال",
            "پوشاک",
            "کتاب",
            "لوازم‌خانگی",
            "ورزشی",
            "زیبایی و بهداشت",
            "خوراکی",
            "اسباب‌بازی"
        ];

        // دسته‌های موجود در دیتابیس هم در نگاشت می‌آیند تا در اجراهای تکمیلی
        // (دیتابیس نیمه‌کاشت‌شده) محصولات جدید هم دسته‌ی درست بگیرند.
        Dictionary<string, Guid> idsByName = await context.Categories
            .ToDictionaryAsync(category => category.Name, category => category.Id, cancellationToken);

        int created = 0;
        foreach (string name in categoryNames)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (idsByName.ContainsKey(name))
            {
                continue;
            }

            var categoryId = Guid.NewGuid();
            context.Categories.Add(new Category
            {
                Id = categoryId,
                Name = name,
                Description = $"دسته‌بندی {name} کاتالوگ.",
                IsActive = true
            });
            idsByName[name] = categoryId;
            created++;
        }

        return (created, idsByName);
    }

    /// <summary>
    /// جدول محصولات را با نام‌های فارسی (متناسب با دسته‌بندی‌ها) تا
    /// <see cref="ProductSeedCount"/> ردیف کامل می‌کند. هر محصول به دسته‌ی مرتبطش متصل
    /// می‌شود تا فیلتر دسته‌بندی از همان ابتدا داده‌ی نمایشی داشته باشد. SKU ها از قالب
    /// یکتای <c>SKU-0001</c> پیروی می‌کنند و هر دهمین محصول (دقیقاً ۱۰٪) غیرفعال است.
    /// توالی قطعی است (seed 12345).
    /// </summary>
    /// <param name="context">کانتکست دیتابیس.</param>
    /// <param name="categoryIdsByName">نگاشت نام دسته به شناسه (خروجی داده‌کاشت دسته‌ها).</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>تعداد محصولاتی که ایجاد شدند.</returns>
    private static async Task<int> SeedProductsAsync(
        OrderManagementDbContext context,
        Dictionary<string, Guid> categoryIdsByName,
        CancellationToken cancellationToken)
    {
        int existingCount = await context.Products.CountAsync(cancellationToken);
        int missingCount = ProductSeedCount - existingCount;
        if (missingCount <= 0)
        {
            return 0;
        }

        (string Name, string Category)[] catalog =
        [
            ("گوشی موبایل", "کالای دیجیتال"),
            ("لپ‌تاپ", "کالای دیجیتال"),
            ("هدفون بی‌سیم", "کالای دیجیتال"),
            ("ساعت هوشمند", "کالای دیجیتال"),
            ("کیبورد مکانیکی", "کالای دیجیتال"),
            ("ماوس گیمینگ", "کالای دیجیتال"),
            ("مانیتور", "کالای دیجیتال"),
            ("تبلت", "کالای دیجیتال"),
            ("اسپیکر بلوتوثی", "کالای دیجیتال"),
            ("پاوربانک", "کالای دیجیتال"),
            ("تیشرت نخی", "پوشاک"),
            ("شلوار جین", "پوشاک"),
            ("کفش ورزشی", "پوشاک"),
            ("کاپشن زمستانی", "پوشاک"),
            ("مانتو", "پوشاک"),
            ("کیف چرمی", "پوشاک"),
            ("کتاب رمان", "کتاب"),
            ("کتاب آشپزی", "کتاب"),
            ("دفتر یادداشت", "کتاب"),
            ("مبل راحتی", "لوازم‌خانگی"),
            ("چراغ مطالعه", "لوازم‌خانگی"),
            ("قابلمه", "لوازم‌خانگی"),
            ("سرویس چای‌خوری", "لوازم‌خانگی"),
            ("دمبل", "ورزشی"),
            ("توپ فوتبال", "ورزشی"),
            ("قمقمه ورزشی", "ورزشی"),
            ("کرم مرطوب‌کننده", "زیبایی و بهداشت"),
            ("شامپو", "زیبایی و بهداشت"),
            ("عسل طبیعی", "خوراکی"),
            ("چای ایرانی", "خوراکی"),
            ("لگوی ساختنی", "اسباب‌بازی"),
            ("عروسک", "اسباب‌بازی"),
        ];

        var productFaker = new Faker("fa") { Random = new Randomizer(BogusSeed) };

        for (int index = existingCount; index < ProductSeedCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            (string baseName, string categoryName) = catalog[index % catalog.Length];
            // وقتی نام‌های پایه تمام شوند، مدل شماره‌دار اضافه می‌شود تا نام‌ها تکراریِ محض نباشند.
            int modelNumber = index / catalog.Length + 1;
            string name = modelNumber == 1 ? baseName : $"{baseName} مدل {modelNumber}";

            int rowNumber = index + 1;
            context.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = $"{name} با کیفیت بالا و ضمانت اصالت کالا.",
                Price = Math.Round(productFaker.Random.Decimal(1m, 1000m), 2),
                StockQuantity = productFaker.Random.Int(0, 500),
                Sku = $"SKU-{rowNumber:D4}",
                // سهم قطعی ۱۰٪ غیرفعال: هر دهمین محصول بازنشسته می‌شود.
                IsActive = rowNumber % 10 != 0,
                CategoryId = categoryIdsByName.TryGetValue(categoryName, out Guid categoryId)
                    ? categoryId
                    : null,
            });
        }

        return missingCount;
    }
}
