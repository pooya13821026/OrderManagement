using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OrderManagement.Application.Dtos.Auth;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// نقطه‌پایان‌های احراز هویت: ثبت‌نام، ورود به سیستم، چرخش توکن refresh، لغو توکن
/// و پروفایل کاربر جاری.
/// </summary>
/// <remarks>
/// کنترلر به‌صورت پیش‌فرض نیازمند احراز هویت است؛ ثبت‌نام، ورود و refresh صراحتاً
/// anonymous هستند چون کلاینت برای به‌دست آوردن (یا تمدید) توکن در وهله‌ی اول به آن‌ها
/// نیاز دارد. لغو توکن و نقطه‌پایان پروفایل به یک توکن دسترسی معتبر نیاز دارند.
/// </remarks>
[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="AuthController"/> می‌سازد.
    /// </summary>
    /// <param name="authService">سرویس احراز هویت.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="authService"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// یک حساب کاربری جدید با نقش پیش‌فرض <c>User</c> ثبت می‌کند و زوج تازه‌صادرشده‌ی
    /// توکن دسترسی/refresh را برمی‌گرداند.
    /// </summary>
    /// <param name="request">payload ثبت‌نام.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>کاربر ایجادشده به‌همراه توکن‌های صادرشده.</returns>
    /// <response code="201">حساب ایجاد شد.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد.</response>
    /// <response code="409">آدرس ایمیل قبلاً ثبت شده است.</response>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        AuthResponse response = await _authService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Me), response);
    }

    /// <summary>
    /// با ایمیل و رمز عبور وارد می‌شود و یک زوج توکن تازه برمی‌گرداند.
    /// </summary>
    /// <param name="request">payload ورود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>توکن‌های صادرشده و پروفایل کاربر.</returns>
    /// <response code="200">اعتبارنامه‌ها پذیرفته شدند.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد.</response>
    /// <response code="403">رمز عبور اشتباه است یا حساب غیرفعال شده است.</response>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        AuthResponse response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// یک توکن refresh معتبر و لغونشده را با یک زوج توکن جدید عوض می‌کند (چرخش).
    /// </summary>
    /// <param name="request">توکن refresh که باید عوض شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>توکن‌های جدید و پروفایل کاربر.</returns>
    /// <response code="200">توکن چرخانده شد.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد یا توکن دیگر قابل استفاده نیست.</response>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        AuthResponse response = await _authService.RefreshAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// توکن refresh داده‌شده را لغو می‌کند تا دیگر نتوان آن را با توکن‌های جدید عوض کرد.
    /// نیازمند فراخواننده‌ی احرازهویت‌شده است (خروج از سیستم / پاک‌سازی توکن).
    /// </summary>
    /// <param name="request">توکن refresh که باید لغو شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>در موفقیت بدون محتوا.</returns>
    /// <response code="204">توکن لغو شد.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="404">هیچ توکن refresh ذخیره‌شده‌ای منطبق با مقدار ارائه‌شده نیست.</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        await _authService.RevokeAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// پروفایل کاربری را که توکن دسترسی ارائه‌شده نماینده‌ی آن است دریافت می‌کند.
    /// </summary>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>پروفایل کاربر جاری.</returns>
    /// <response code="200">پروفایل برگردانده شد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="404">حساب پشت توکن دیگر وجود ندارد.</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        // claim مربوط به "sub" در JWT به‌صورت ClaimTypes.NameIdentifier نگاشت می‌شود
        // (نگاشت claim های ورودی که با middleware مربوط به bearer پیکربندی شده است).
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "توکن دسترسی نامعتبر",
                detail: "توکن دسترسی شامل شناسه‌ی کاربر قابل استفاده نیست.");
        }

        UserDto user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        return Ok(user);
    }
}
