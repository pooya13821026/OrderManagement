using Microsoft.AspNetCore.Mvc;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Api.Middleware;

/// <summary>
/// جزئی از پایپ‌لاین که هر استثنای رها شده از کنترلرها و سرویس‌های اپلیکیشن را می‌گیرد و به
/// پاسخ <c>application/problem+json</c> طبق RFC 7807 تبدیل می‌کند. استثناهای دامنه نگاشت صریح
/// دارند (404/409/400/403)؛ هر خطای پیش‌بینی‌نشده به 500 عمومی تبدیل می‌شود که جزئیات داخلی را
/// به فراخواننده لو نمی‌دهد. خطاها از طریق <c>ILogger</c> ثبت می‌شوند که Serilog با
/// <c>AddSerilog</c> آن را دریافت می‌کند.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// نمونه جدیدی از <see cref="ExceptionHandlingMiddleware"/> می‌سازد.
    /// </summary>
    /// <param name="next">جزء بعدی پایپ‌لاین.</param>
    /// <param name="logger">لاگری که خطاها را ثبت می‌کند.</param>
    /// <exception cref="ArgumentNullException">وقتی یکی از آرگومان‌ها <c>null</c> باشد پرتاب می‌شود.</exception>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// بقیه پایپ‌لاین را اجرا می‌کند و هر استثنای پرتاب‌شده را به پاسخ مشکل (problem) تبدیل می‌کند.
    /// </summary>
    /// <param name="context">کانتکست جاری HTTP.</param>
    /// <returns>تسکی که نشان‌دهنده عملیات ناهمگام است.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                // بدنه پاسخ قبلاً روی سیم رفته است: تنها راه امن این است که بگذاریم
                // هاست اتصال را قطع کند، پس استثنای اصلی دوباره پرتاب می‌شود.
                _logger.LogWarning(
                    exception,
                    "پاسخ {Method} {Path} قبلاً شروع شده بود؛ پرتاب مجدد.",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }

            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// بدنه جزئیات-مشکل (problem-details) متناظر با استثنا را می‌نویسد.
    /// </summary>
    /// <param name="context">کانتکست جاری HTTP.</param>
    /// <param name="exception">استثنایی که باید ترجمه شود.</param>
    /// <returns>تسکی که نشان‌دهنده عملیات ناهمگام است.</returns>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        (int statusCode, string title, string detail) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            // خطای پیش‌بینی‌نشده: جزئیات کامل لاگ می‌شود ولی فقط پیام عمومی به بیرون داده می‌شود.
            _logger.LogError(
                exception,
                "استثنای مدیریت‌نشده هنگام پردازش {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            // خروجی شناخته‌شده دامنه (404/409/400/403): پیام برای فراخواننده در نظر گرفته شده است.
            _logger.LogWarning(
                exception,
                "درخواست {Method} {Path} با کد {StatusCode} ناموفق بود.",
                context.Request.Method,
                context.Request.Path,
                statusCode);
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

        context.Response.Clear();
        context.Response.StatusCode = statusCode;

        // contentType باید صریح پاس داده شود؛ وگرنه WriteAsJsonAsync مقدار پیش‌فرض
        // application/json را جایگزین problem+json می‌کند.
        await context.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json");
    }

    /// <summary>
    /// هر استثنا را به کد وضعیت HTTP، عنوان پاسخ و جزئیات قابل نمایش به کاربر تبدیل می‌کند.
    /// استثناهای مشتق‌شده دامنه قبل از <see cref="DomainException"/> پایه تطبیق داده می‌شوند.
    /// </summary>
    /// <param name="exception">استثنایی که باید نگاشت شود.</param>
    /// <returns>کد وضعیت، عنوان و جزئیات پاسخ مشکل.</returns>
    private static (int StatusCode, string Title, string Detail) MapException(Exception exception) => exception switch
    {
        EntityNotFoundException notFound =>
            (StatusCodes.Status404NotFound, "منبع یافت نشد", notFound.Message),
        DuplicateEntityException duplicate =>
            (StatusCodes.Status409Conflict, "منبع قبلاً وجود دارد", duplicate.Message),
        InsufficientStockException stock =>
            (StatusCodes.Status409Conflict, "موجودی ناکافی است", stock.Message),
        InvalidStatusTransitionException transition =>
            (StatusCodes.Status400BadRequest, "تغییر وضعیت نامعتبر است", transition.Message),
        UnauthorizedOperationException unauthorized =>
            (StatusCodes.Status403Forbidden, "عملیات مجاز نیست", unauthorized.Message),
        DomainException domain =>
            (StatusCodes.Status400BadRequest, "قانون کسب‌وکار نقض شد", domain.Message),
        _ =>
            (StatusCodes.Status500InternalServerError,
                "خطای داخلی سرور",
                "هنگام پردازش درخواست خطای پیش‌بینی‌نشده‌ای رخ داد.")
    };
}
