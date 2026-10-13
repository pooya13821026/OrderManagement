using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Customers;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// نقطه‌پایان‌های مدیریت مشتریان: ایجاد، جست‌وجوی صفحه‌بندی‌شده، بازیابی، به‌روزرسانی و حذف.
/// </summary>
/// <remarks>
/// خواندن برای هر کاربر احرازهویت‌شده باز است؛ عملیات نوشتن (POST، PUT، DELETE) از طریق
/// یک الزام نقش اضافی فقط به مدیران محدود شده است.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="CustomersController"/> می‌سازد.
    /// </summary>
    /// <param name="customerService">سرویس مشتریان.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="customerService"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
    }

    /// <summary>
    /// یک مشتری جدید ایجاد می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="request">payload ایجاد مشتری.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>مشتری ایجادشده.</returns>
    /// <response code="201">مشتری ایجاد شد.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="409">آدرس ایمیل قبلاً اشغال شده است.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> Create(
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        CustomerDto created = await _customerService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// یک صفحه‌ی فیلترشده و صفحه‌بندی‌شده از مشتریان دریافت می‌کند.
    /// </summary>
    /// <param name="parameters">پارامترهای پرس‌وجو <c>pageNumber</c> و <c>pageSize</c>.</param>
    /// <param name="searchTerm">عبارت اختیاری که با نام و ایمیل مقایسه می‌شود.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>صفحه‌ی درخواستی مشتریان.</returns>
    /// <response code="200">صفحه برگردانده شد.</response>
    /// <response code="400">یک پارامتر پرس‌وجو نامعتبر بود.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetPaged(
        [FromQuery] PaginationParams parameters,
        [FromQuery] string? searchTerm,
        CancellationToken cancellationToken)
    {
        PagedResult<CustomerDto> page = await _customerService.GetPagedAsync(
            parameters,
            searchTerm,
            cancellationToken);

        return Ok(page);
    }

    /// <summary>
    /// یک مشتری را بر اساس شناسه دریافت می‌کند.
    /// </summary>
    /// <param name="id">شناسه‌ی مشتری.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>مشتری درخواستی.</returns>
    /// <response code="200">مشتری برگردانده شد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="404">مشتری‌ای با این شناسه وجود ندارد.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        CustomerDto customer = await _customerService.GetByIdAsync(id, cancellationToken);
        return Ok(customer);
    }

    /// <summary>
    /// یک مشتری موجود را به‌روزرسانی می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="id">شناسه‌ی مشتری.</param>
    /// <param name="request">payload به‌روزرسانی مشتری.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>مشتری به‌روزرسانی‌شده.</returns>
    /// <response code="200">مشتری به‌روزرسانی شد.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="404">مشتری‌ای با این شناسه وجود ندارد.</response>
    /// <response code="409">آدرس ایمیل قبلاً اشغال شده است.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> Update(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        CustomerDto updated = await _customerService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// یک مشتری را حذف می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="id">شناسه‌ی مشتری.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>در موفقیت بدون محتوا.</returns>
    /// <response code="204">مشتری حذف شد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="404">مشتری‌ای با این شناسه وجود ندارد.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _customerService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
