using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Categories;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// نقطه‌پایان‌های مدیریت دسته‌بندی محصولات: ایجاد، جست‌وجوی صفحه‌بندی‌شده، بازیابی، به‌روزرسانی و حذف.
/// </summary>
/// <remarks>
/// خواندن برای هر کاربر احرازهویت‌شده باز است؛ عملیات نوشتن (POST) از طریق
/// یک الزام نقش اضافی فقط به مدیران محدود شده است.
/// </remarks>
[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="CategoriesController"/> می‌سازد.
    /// </summary>
    /// <param name="categoryService">سرویس دسته‌بندی‌ها.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="categoryService"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
    }

    /// <summary>
    /// یک دسته‌بندی جدید ایجاد می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="request">payload ایجاد دسته.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>دسته ایجادشده.</returns>
    /// <response code="201">دسته ایجاد شد.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="409">نام دسته قبلاً اشغال شده است.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        CategoryDto created = await _categoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// یک صفحه‌ی فیلترشده و صفحه‌بندی‌شده از دسته‌بندی‌ها دریافت می‌کند.
    /// </summary>
    /// <param name="parameters">پارامترهای پرس‌وجو <c>pageNumber</c> و <c>pageSize</c>.</param>
    /// <param name="searchTerm">عبارت اختیاری که با نام و توضیحات مقایسه می‌شود.</param>
    /// <param name="isActive">فیلتر اختیاری پرچم فعال بودن؛ خالی = همه.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>صفحه‌ی درخواستی دسته‌ها.</returns>
    /// <response code="200">صفحه برگردانده شد.</response>
    /// <response code="400">یک پارامتر پرس‌وجو نامعتبر بود.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<CategoryDto>>> GetPaged(
        [FromQuery] PaginationParams parameters,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        PagedResult<CategoryDto> page = await _categoryService.GetPagedAsync(
            parameters,
            searchTerm,
            isActive,
            cancellationToken);

        return Ok(page);
    }

    /// <summary>
    /// یک دسته را بر اساس شناسه دریافت می‌کند.
    /// </summary>
    /// <param name="id">شناسه‌ی دسته.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>دسته درخواستی.</returns>
    /// <response code="200">دسته برگردانده شد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="404">دسته‌ای با این شناسه وجود ندارد.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        CategoryDto category = await _categoryService.GetByIdAsync(id, cancellationToken);
        return Ok(category);
    }

    /// <summary>
    /// یک دسته موجود را به‌روزرسانی می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="id">شناسه‌ی دسته.</param>
    /// <param name="request">payload به‌روزرسانی دسته.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>دسته به‌روزرسانی‌شده.</returns>
    /// <response code="200">دسته به‌روزرسانی شد.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="404">دسته‌ای با این شناسه وجود ندارد.</response>
    /// <response code="409">نام دسته قبلاً اشغال شده است.</response>
    [HttpPost("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryDto>> Update(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        CategoryDto updated = await _categoryService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// یک دسته را حذف می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="id">شناسه‌ی دسته.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>در موفقیت بدون محتوا.</returns>
    /// <response code="204">دسته حذف شد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="404">دسته‌ای با این شناسه وجود ندارد.</response>
    [HttpPost("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _categoryService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
