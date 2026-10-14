using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Auth;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// نقطه‌پایان‌های مدیریت کاربران: فهرست صفحه‌بندی‌شده‌ی حساب‌ها.
/// </summary>
/// <remarks>
/// فقط مدیران: فهرست کاربران داده‌ی حساس است و کاربر عادی نباید حساب‌های دیگر را ببیند.
/// پروفایل خودِ فراخواننده از <c>api/Auth/Me</c> گرفته می‌شود.
/// </remarks>
[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="UsersController"/> می‌سازد.
    /// </summary>
    /// <param name="authService">سرویس احراز هویت.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="authService"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public UsersController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// یک صفحه‌ی فیلترشده و صفحه‌بندی‌شده از حساب‌های کاربری دریافت می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="parameters">پارامترهای پرس‌وجو <c>pageNumber</c> و <c>pageSize</c>.</param>
    /// <param name="searchTerm">عبارت اختیاری که با نام کاربری، ایمیل و نام مقایسه می‌شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>صفحه‌ی درخواستی کاربران.</returns>
    /// <response code="200">صفحه برگردانده شد.</response>
    /// <response code="400">یک پارامتر پرس‌وجو نامعتبر بود.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetPaged(
        [FromQuery] PaginationParams parameters,
        [FromQuery] string? searchTerm,
        CancellationToken cancellationToken)
    {
        PagedResult<UserDto> page = await _authService.GetPagedUsersAsync(
            parameters,
            searchTerm,
            cancellationToken);

        return Ok(page);
    }
}
