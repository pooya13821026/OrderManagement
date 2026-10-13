using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Customers;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// Customer management endpoints: create, paged search, retrieval, update and delete.
/// </summary>
/// <remarks>
/// Reading is open to any authenticated user; write operations (POST, PUT, DELETE) are
/// restricted to administrators via an additional role requirement.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomersController"/> class.
    /// </summary>
    /// <param name="customerService">The customer service.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="customerService"/> is <c>null</c>.</exception>
    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
    }

    /// <summary>
    /// Creates a new customer. Administrators only.
    /// </summary>
    /// <param name="request">The customer creation payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created customer.</returns>
    /// <response code="201">The customer was created.</response>
    /// <response code="400">The payload failed validation.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="403">The caller is not an administrator.</response>
    /// <response code="409">The email address is already taken.</response>
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
    /// Gets a filtered, paginated page of customers.
    /// </summary>
    /// <param name="parameters">Query parameters <c>pageNumber</c> and <c>pageSize</c>.</param>
    /// <param name="searchTerm">Optional free-text term matched against name and email.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The requested page of customers.</returns>
    /// <response code="200">The page was returned.</response>
    /// <response code="400">A query parameter was invalid.</response>
    /// <response code="401">No valid access token was presented.</response>
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
    /// Gets a single customer by identifier.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The requested customer.</returns>
    /// <response code="200">The customer was returned.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="404">No customer with the given identifier exists.</response>
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
    /// Updates an existing customer. Administrators only.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="request">The customer update payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated customer.</returns>
    /// <response code="200">The customer was updated.</response>
    /// <response code="400">The payload failed validation.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="403">The caller is not an administrator.</response>
    /// <response code="404">No customer with the given identifier exists.</response>
    /// <response code="409">The email address is already taken.</response>
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
    /// Deletes a customer. Administrators only.
    /// </summary>
    /// <param name="id">The customer identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">The customer was deleted.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="403">The caller is not an administrator.</response>
    /// <response code="404">No customer with the given identifier exists.</response>
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
