using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OrderManagement.Api.Serialization;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using Serilog;

namespace OrderManagement.Api.Extensions;

/// <summary>
/// نقطه‌های ثبت DI لایه API: کنترلرهای MVC و Swagger، احراز هویت JWT حامل (Bearer)،
/// Serilog و ثبت سرویس‌های اپلیکیشن.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// کنترلرهای MVC، Swagger (همراه با دکمه Authorize حامل و توضیحات XML)، احراز هویت
    /// حامل JWT بر اساس بخش <c>Jwt</c> تنظیمات، مجوزدهی و لاگ ساخت‌یافته/درخواستی Serilog را
    /// اضافه می‌کند. تنظیمات JWT از همان بخشی خوانده می‌شود که <c>JwtTokenService</c> با آن
    /// امضا می‌کند، پس توکن صادرشده و اعتبارسنجی‌شده همیشه با هم سازگارند.
    /// </summary>
    /// <param name="services">کالکشن سرویس‌ها.</param>
    /// <param name="configuration">تنظیمات اپلیکیشن؛ بخش <c>Jwt</c> را می‌خواند.</param>
    /// <returns>کالکشن سرویس‌ها، برای زنجیره‌سازی.</returns>
    /// <exception cref="ArgumentNullException">وقتی یکی از آرگومان‌ها <c>null</c> باشد پرتاب می‌شود.</exception>
    /// <exception cref="InvalidOperationException">وقتی <c>Jwt:Key</c> خالی یا غایب باشد پرتاب می‌شود.</exception>
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddControllers()
            .AddJsonOptions(options =>
            {
                // همه‌ی تاریخ‌های خروجی فقط تا دقت دقیقه نمایش داده می‌شوند (بدون ثانیه).
                options.JsonSerializerOptions.Converters.Add(new MinutePrecisionDateTimeConverter());
            });
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Order Management API",
                Version = "v1",
                Description = "ای‌پی‌آی سفارش‌ها، مشتریان و محصولات، محافظت‌شده با احراز هویت حامل JWT."
            });

            // دکمه «Authorize» را به Swagger UI اضافه می‌کند: توکن را بچسبانید،
            // آن‌گاه هر درخواست سربرگ «Authorization: Bearer <token>» را حمل می‌کند.
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "توکن دسترسی JWT. خود توکن خام را وارد کنید؛ پیشوند «Bearer » را خود Swagger UI اضافه می‌کند."
            });

            // نیازمندی امنیتی در برابر سند تولیدشده حل می‌شود تا به طرح تعریف‌شده بالا اشاره کند.
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
            });

            // مستندات XML درون‌خطی را منتشر می‌کند وقتی پروژه‌ای آن را تولید کند؛ Api همیشه
            // تولید می‌کند، لایه‌های دیگر فقط وقتی GenerateDocumentationFile فعال باشد.
            foreach (string fileName in new[] { "Api.xml", "Application.xml", "Domain.xml", "Infrastructure.xml" })
            {
                string path = Path.Combine(AppContext.BaseDirectory, fileName);
                if (File.Exists(path))
                {
                    options.IncludeXmlComments(path);
                }
            }
        });

        IConfigurationSection jwtSection = configuration.GetSection("Jwt");
        string issuer = jwtSection["Issuer"] ?? "OrderManagement";
        string audience = jwtSection["Audience"] ?? "OrderManagementClients";
        string key = jwtSection["Key"]
            ?? throw new InvalidOperationException("مقدار الزامی تنظیمات 'Jwt:Key' یافت نشد.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),

                    // توکن‌ها نقش را در ادعای کوتاه «role» حمل می‌کنند؛ میان‌افزار حامل آن را
                    // روی ClaimTypes.Role می‌نگارد (نگاشت ورودی ادعا) و RoleClaimType به
                    // [Authorize(Roles = ...)] می‌گوید نقش را از کجا بخواند.
                    RoleClaimType = ClaimTypes.Role
                };

                // پاسخ‌های ۴۰۱ (چالش) و ۴۰۳ (ممنوعیت) خود چارچوب هم به همان شکل
                // ProblemDetails بقیه‌ی خطاها نوشته می‌شوند تا قرارداد خطای API برای
                // مصرف‌کننده یک‌دست بماند و مستندات Swagger با واقعیت بخواند.
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // پاسخ پیش‌فرض چارچوب (۴۰۱ بدون بدنه) سرکوب می‌شود؛ در عوض بدنه‌ی
                        // problem+json می‌نویسیم و سربرگ RFC 6750 را خودمان می‌گذاریم.
                        context.HandleResponse();
                        context.Response.Headers.WWWAuthenticate =
                            context.AuthenticateFailure is null ? "Bearer" : "Bearer error=\"invalid_token\"";
                        await WriteProblemDetailsAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "احراز هویت لازم است",
                            "هیچ توکن دسترسی معتبری ارائه نشده است.");
                    },

                    // برای ممنوعیت، چارچوب کد ۴۰۳ را پیش از رویداد ست می‌کند و پس از آن
                    // کاری نمی‌کند؛ پس فقط بدنه نوشته می‌شود.
                    OnForbidden = context => WriteProblemDetailsAsync(
                        context.HttpContext,
                        StatusCodes.Status403Forbidden,
                        "عملیات مجاز نیست",
                        "شما مجوز دسترسی به این منبع را ندارید.")
                };
            });

        services.AddAuthorization();

        // لاگ Serilog سطوح حداقلی و مقصدها (کنسول و فایل چرخشی) را از appsettings.json می‌خواند.
        services.AddSerilog((_, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(configuration));

        return services;
    }

    /// <summary>
    /// سرویس‌های اپلیکیشن (محدوده درخواست) که کنترلرها به آن‌ها وابسته‌اند را ثبت می‌کند.
    /// وابستگی‌های زیرساختی آن‌ها (واحد کار، ریپازیتوری‌ها، سرویس توکن) جداگانه با
    /// <c>AddInfrastructure</c> ثبت می‌شوند.
    /// </summary>
    /// <param name="services">کالکشن سرویس‌ها.</param>
    /// <returns>کالکشن سرویس‌ها، برای زنجیره‌سازی.</returns>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="services"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }

    /// <summary>
    /// پاسخ RFC 7807 (<c>application/problem+json</c>) هم‌شکل با میان‌افزار خطای سراسری
    /// می‌نویسد؛ برای ۴۰۱ چالش و ۴۰۳ ممنوعیتی که هرگز به میان‌افزار استثنا نمی‌رسند.
    /// </summary>
    /// <param name="context">کانتکست درخواست جاری.</param>
    /// <param name="statusCode">کد وضعیت HTTP.</param>
    /// <param name="title">عنوان کوتاه پاسخ مشکل.</param>
    /// <param name="detail">توضیح قابل نمایش به فراخواننده.</param>
    /// <returns>تسکی که نشان‌دهنده عملیات ناهمگام است.</returns>
    private static async Task WriteProblemDetailsAsync(HttpContext context, int statusCode, string title, string detail)
    {
        // دفاع در برابر استفاده‌ی آینده: اگر پاسخ قبلاً شروع شده باشد نوشتن دوباره
        // ممکن نیست؛ مثل میان‌افزار خطا کوتاه می‌شویم تا خرابی بدنه نسازیم.
        if (context.Response.HasStarted)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        // پاسخ را با ورودی‌های لاگ سمت سرور برای همین درخواست مرتبط می‌کند.
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;

        // contentType باید صریح پاس داده شود؛ وگرنه WriteAsJsonAsync مقدار پیش‌فرض
        // application/json را جایگزین problem+json می‌کند.
        await context.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json");
    }
}
