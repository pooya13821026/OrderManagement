using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Orders;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// Order lifecycle endpoints: create, paged search, retrieval, status transitions,
/// deletion and bulk creation.
/// </summary>
/// <remarks>
/// Every action requires an authenticated caller. Deletion and bulk creation are further
/// restricted to administrators; status transitions forward the caller's role to the
/// application service, which enforces the admin-only <c>Cancelled</c> transition.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="orderService">The order service.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="orderService"/> is <c>null</c>.</exception>
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
    }

    /// <summary>
    /// Creates a new order in the <c>Pending</c> status. Open to any authenticated user.
    /// </summary>
    /// <param name="request">The order creation payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">The order was created.</response>
    /// <response code="400">The payload failed validation or a business rule was violated.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="404">The customer or one of the products does not exist.</response>
    /// <response code="409">There is not enough stock for one of the products.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        OrderDto created = await _orderService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Gets a filtered, sorted and paginated page of orders.
    /// </summary>
    /// <param name="filter">
    /// Query parameters: <c>customerId</c>, <c>status</c>, <c>fromDate</c>, <c>toDate</c>,
    /// <c>searchTerm</c>, <c>sortBy</c>, <c>sortDescending</c>, <c>pageNumber</c>, <c>pageSize</c>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The requested page of orders.</returns>
    /// <response code="200">The page was returned.</response>
    /// <response code="400">A query parameter was invalid.</response>
    /// <response code="401">No valid access token was presented.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<OrderDto>>> GetPaged(
        [FromQuery] OrderFilterParams filter,
        CancellationToken cancellationToken)
    {
        PagedResult<OrderDto> page = await _orderService.GetPagedAsync(filter, cancellationToken);
        return Ok(page);
    }

    /// <summary>
    /// Gets a single order by identifier, including customer and line items.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The requested order.</returns>
    /// <response code="200">The order was returned.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="404">No order with the given identifier exists.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        OrderDto order = await _orderService.GetByIdAsync(id, cancellationToken);
        return Ok(order);
    }

    /// <summary>
    /// Transitions an order to a new status (forward-only rules are enforced by the service).
    /// Any authenticated user may progress an order; cancelling it additionally requires the
    /// <c>Admin</c> role, which is why the caller's role claim is forwarded to the service.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="request">The requested new status.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated order.</returns>
    /// <response code="200">The status was updated.</response>
    /// <response code="400">The payload failed validation or the transition is not allowed.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="403">A non-administrator tried to cancel the order.</response>
    /// <response code="404">No order with the given identifier exists.</response>
    /// <response code="409">Confirming the order would exceed the available stock.</response>
    [HttpPatch("{id:guid}/status")]
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
        // Role claim: the JWT "role" claim is surfaced as ClaimTypes.Role by the bearer
        // middleware (RoleClaimType). The service compares it against Roles.Admin to allow
        // the admin-only Cancelled transition.
        string requesterRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        OrderDto updated = await _orderService.UpdateStatusAsync(
            id,
            request.NewStatus,
            requesterRole,
            cancellationToken);

        return Ok(updated);
    }

    /// <summary>
    /// Deletes an order. Administrators only.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">The order was deleted.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="403">The caller is not an administrator.</response>
    /// <response code="404">No order with the given identifier exists.</response>
    [HttpDelete("{id:guid}")]
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
    /// Creates many orders in one call, reporting a per-order success or failure.
    /// Administrators only.
    /// </summary>
    /// <param name="request">The bulk creation payload (1-500 orders).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The per-order results.</returns>
    /// <response code="200">The bulk operation ran; inspect the per-order results.</response>
    /// <response code="400">The payload failed validation.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="403">The caller is not an administrator.</response>
    [HttpPost("bulk")]
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
}
