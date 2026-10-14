using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// نقطه‌پایان‌های چرخه‌ی حیات سفارش: ایجاد، جست‌وجوی صفحه‌بندی‌شده، بازیابی، گذارهای
/// وضعیت، حذف و ایجاد گروهی.
/// </summary>
/// <remarks>
/// همه‌ی اکشن‌ها نیازمند فراخواننده‌ی احرازهویت‌شده‌اند. حذف و ایجاد گروهی علاوه بر این
/// فقط برای مدیران مجازند؛ گذارهای وضعیت نقش فراخواننده را به سرویس برنامه منتقل می‌کنند
/// که گذار <c>Cancelled</c> مخصوص مدیر را اعمال می‌کند.
/// </remarks>
[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="OrdersController"/> می‌سازد.
    /// </summary>
    /// <param name="orderService">سرویس سفارش‌ها.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="orderService"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    /// <summary>
    /// یک سفارش جدید با وضعیت <c>Pending</c> ایجاد می‌کند. کاربر عادی فقط برای
    /// مشتریِ متصل به حساب خودش می‌تواند سفارش ثبت کند (در غیر این صورت ۴۰۳)؛ مدیر
    /// برای هر مشتری معتبر.
    /// </summary>
    /// <param name="request">payload ایجاد سفارش.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>سفارش ایجادشده.</returns>
    /// <response code="201">سفارش ایجاد شد.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد یا قاعده‌ی کسب‌وکار نقض شد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">مشتریِ درخواستی متعلق به فراخواننده نیست.</response>
    /// <response code="404">مشتری یکی از محصولات وجود ندارد.</response>
    /// <response code="409">موجودی یکی از محصولات کافی نیست.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        OrderDto created = await _orderService.CreateAsync(
            request,
            ReadCallerUserId() ?? Guid.Empty,
            ReadCallerRole(),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// یک صفحه‌ی فیلترشده، مرتب‌شده و صفحه‌بندی‌شده از سفارش‌ها دریافت می‌کند.
    /// کاربر عادی فقط سفارش‌های خودش را می‌بیند (فیلتر مشتری نادیده گرفته می‌شود)؛
    /// مدیر همه‌ی سفارش‌ها را با فیلتر کامل می‌بیند.
    /// </summary>
    /// <param name="filter">
    /// پارامترهای پرس‌وجو: <c>customerId</c>، <c>status</c>، <c>fromDate</c>، <c>toDate</c>،
    /// <c>searchTerm</c>، <c>sortBy</c>، <c>sortDescending</c>، <c>pageNumber</c>، <c>pageSize</c>.
    /// </param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>صفحه‌ی درخواستی سفارش‌ها.</returns>
    /// <response code="200">صفحه برگردانده شد.</response>
    /// <response code="400">یک پارامتر پرس‌وجو نامعتبر بود.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetPaged(
        [FromQuery] OrderFilterParams filter,
        CancellationToken cancellationToken)
    {
        PagedResult<OrderDto> page = await _orderService.GetPagedAsync(
            filter,
            ReadCallerUserId(),
            ReadCallerRole(),
            cancellationToken);
        return Ok(page);
    }

    /// <summary>
    /// یک سفارش را بر اساس شناسه، شامل مشتری و ردیف‌ها، دریافت می‌کند.
    /// کاربر عادی فقط سفارش خودش را می‌بیند (در غیر این صورت ۴۰۳).
    /// </summary>
    /// <param name="id">شناسه‌ی سفارش.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>سفارش درخواستی.</returns>
    /// <response code="200">سفارش برگردانده شد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">سفارش متعلق به فراخواننده نیست.</response>
    /// <response code="404">سفارشی با این شناسه وجود ندارد.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        OrderDto order = await _orderService.GetByIdAsync(id, ReadCallerUserId(), ReadCallerRole(), cancellationToken);
        return Ok(order);
    }

    /// <summary>
    /// یک سفارش را به وضعیت جدید منتقل می‌کند (قواعد forward-only توسط سرویس اعمال می‌شوند).
    /// هر کاربر احرازهویت‌شده می‌تواند سفارش را جلو ببرد؛ لغو آن علاوه بر این به نقش
    /// <c>Admin</c> نیاز دارد، به همین دلیل claim نقش فراخواننده به سرویس منتقل می‌شود.
    /// شناسه‌ی کاربر فراخواننده هم برای ثبت در تاریخچه‌ی وضعیت ارسال می‌شود و فیلد
    /// اختیاری <c>reason</c> (فقط برای لغو معنا دارد) دلیل را در تاریخچه و <c>CancelReason</c> می‌نویسد.
    /// </summary>
    /// <param name="id">شناسه‌ی سفارش.</param>
    /// <param name="request">وضعیت جدید درخواستی به‌همراه دلیل اختیاری.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>سفارش به‌روزرسانی‌شده.</returns>
    /// <response code="200">وضعیت به‌روزرسانی شد.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد یا گذار مجاز نیست.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">یک غیرمدیر سعی کرد سفارش را لغو کند.</response>
    /// <response code="404">سفارشی با این شناسه وجود ندارد.</response>
    /// <response code="409">تأیید سفارش از موجودی در دسترس فراتر می‌رود.</response>
    [HttpPost("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        OrderDto updated = await _orderService.UpdateStatusAsync(
            id,
            request.NewStatus,
            ReadCallerRole(),
            cancellationToken,
            request.Reason,
            ReadCallerUserId());

        return Ok(updated);
    }

    /// <summary>
    /// یک سفارش را حذف می‌کند. فقط مدیران.
    /// </summary>
    /// <param name="id">شناسه‌ی سفارش.</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>در موفقیت بدون محتوا.</returns>
    /// <response code="204">سفارش حذف شد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    /// <response code="404">سفارشی با این شناسه وجود ندارد.</response>
    [HttpPost("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _orderService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// چندین سفارش را در یک فراخوانی ایجاد می‌کند و موفقیت یا شکست هر سفارش را گزارش می‌دهد.
    /// فقط مدیران.
    /// </summary>
    /// <param name="request">payload ایجاد گروهی (۱ تا ۵۰۰ سفارش).</param>
    /// <param name="cancellationToken">توکنی برای لغو عملیات.</param>
    /// <returns>نتایج به تفکیک سفارش.</returns>
    /// <response code="200">عملیات گروهی اجرا شد؛ نتایج هر سفارش را بررسی کنید.</response>
    /// <response code="400">payload اعتبارسنجی را رد کرد.</response>
    /// <response code="401">هیچ توکن دسترسی معتبری ارائه نشده است.</response>
    /// <response code="403">فراخواننده مدیر نیست.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(BulkCreateOrdersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BulkCreateOrdersResponse>> BulkCreate(
        [FromBody] BulkCreateOrdersRequest request,
        CancellationToken cancellationToken)
    {
        BulkCreateOrdersResponse response = await _orderService.BulkCreateAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// نقش فراخواننده را از claim اعتبارسنجی‌شده‌ی توکن می‌خواند. claim "role" در JWT
    /// توسط middleware حامل به ClaimTypes.Role نمایان می‌شود (RoleClaimType).
    /// </summary>
    /// <returns>نقش فراخواننده، یا رشته‌ی خالی وقتی claim ندارد.</returns>
    private string ReadCallerRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }

    /// <summary>
    /// شناسه‌ی فراخواننده را از claim اعتبارسنجی‌شده‌ی توکن می‌خواند (claim sub در JWT
    /// به ClaimTypes.NameIdentifier نگاشته می‌شود). برای مالکیت سفارش و ثبت تاریخچه
    /// استفاده می‌شود؛ نامعتبر بودن آن فقط یعنی مالکیت اثبات‌نشده (<c>null</c>).
    /// </summary>
    /// <returns>شناسه‌ی فراخواننده یا <c>null</c>.</returns>
    private Guid? ReadCallerUserId()
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid userId)
            ? userId
            : null;
    }
}
