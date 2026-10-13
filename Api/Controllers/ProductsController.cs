using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Products;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// اندپوینت‌های کاتالوگ محصول: ایجاد، جست‌وجوی صفحه‌بندی‌شده، دریافت تکی، ویرایش، حذف و اصلاح موجودی.
/// </summary>
/// <remarks>
/// خواندن برای هر کاربر احراز هویت‌شده آزاد است؛ عملیات نوشتنی (POST، PUT، DELETE) با
/// نیازمندی نقش اضافه فقط برای مدیران مجاز است.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    /// <summary>
    /// نمونه جدیدی از <see cref="ProductsController"/> می‌سازد.
    /// </summary>
    /// <param name="productService">سرویس محصول.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="productService"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public ProductsController(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    /// <summary>
    /// یک محصول جدید ایجاد می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="request">بدنه درخواست ایجاد محصول.</param>
    /// <param name="cancellationToken">توکن لغو عملیات.</param>
    /// <returns>محصول ایجادشده.</returns>
    /// <response code="201">محصول ایجاد شد.</response>
    /// <response code="400">بدنه درخواست اعتبارسنجی نشد.</response>
    /// <response code="401">توکن دسترسی معتبر ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="409">کد کالا (SKU) قبلاً استفاده شده است.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Create(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        ProductDto created = await _productService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// یک صفحه فیلترشده و صفحه‌بندی‌شده از محصولات را برمی‌گرداند.
    /// </summary>
    /// <param name="filter">
    /// پارامترهای کوئری: <c>searchTerm</c>، <c>isActive</c>، <c>pageNumber</c>، <c>pageSize</c>.
    /// </param>
    /// <param name="cancellationToken">توکن لغو عملیات.</param>
    /// <returns>صفحه درخواستی محصولات.</returns>
    /// <response code="200">صفحه برگردانده شد.</response>
    /// <response code="400">یکی از پارامترهای کوئری نامعتبر است.</response>
    /// <response code="401">توکن دسترسی معتبر ارائه نشده است.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetPaged(
        [FromQuery] ProductFilterParams filter,
        CancellationToken cancellationToken)
    {
        PagedResult<ProductDto> page = await _productService.GetPagedAsync(filter, cancellationToken);
        return Ok(page);
    }

    /// <summary>
    /// یک محصول را با شناسه برمی‌گرداند.
    /// </summary>
    /// <param name="id">شناسه محصول.</param>
    /// <param name="cancellationToken">توکن لغو عملیات.</param>
    /// <returns>محصول درخواستی.</returns>
    /// <response code="200">محصول برگردانده شد.</response>
    /// <response code="401">توکن دسترسی معتبر ارائه نشده است.</response>
    /// <response code="404">محصولی با این شناسه وجود ندارد.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        ProductDto product = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(product);
    }

    /// <summary>
    /// یک محصول موجود را ویرایش می‌کند (موجودی از مسیر جداگانه اصلاح می‌شود، نه اینجا).
    /// فقط مدیران.
    /// </summary>
    /// <param name="id">شناسه محصول.</param>
    /// <param name="request">بدنه درخواست ویرایش محصول.</param>
    /// <param name="cancellationToken">توکن لغو عملیات.</param>
    /// <returns>محصول ویرایش‌شده.</returns>
    /// <response code="200">محصول ویرایش شد.</response>
    /// <response code="400">بدنه درخواست اعتبارسنجی نشد.</response>
    /// <response code="401">توکن دسترسی معتبر ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="404">محصولی با این شناسه وجود ندارد.</response>
    /// <response code="409">کد کالا (SKU) قبلاً استفاده شده است.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        ProductDto updated = await _productService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// یک محصول را حذف می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="id">شناسه محصول.</param>
    /// <param name="cancellationToken">توکن لغو عملیات.</param>
    /// <returns>در صورت موفقیت بدون بدنه.</returns>
    /// <response code="204">محصول حذف شد.</response>
    /// <response code="401">توکن دسترسی معتبر ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="404">محصولی با این شناسه وجود ندارد.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _productService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
