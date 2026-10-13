using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common;
using OrderManagement.Application.Dtos.Products;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// Product catalog endpoints: create, paged search, retrieval, update, delete and stock adjustment.
/// </summary>
/// <remarks>
/// Reading is open to any authenticated user; write operations (POST, PUT, DELETE) are
/// restricted to administrators via an additional role requirement.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="productService">The product service.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="productService"/> is <c>null</c>.</exception>
    public ProductsController(IProductService productService)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
    }

    /// <summary>
    /// Creates a new product. Administrators only.
    /// </summary>
    /// <param name="request">The product creation payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created product.</returns>
    /// <response code="201">The product was created.</response>
    /// <response code="400">The payload failed validation.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="403">The caller is not an administrator.</response>
    /// <response code="409">The SKU is already taken.</response>
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
    /// Gets a filtered, paginated page of products.
    /// </summary>
    /// <param name="filter">
    /// Query parameters: <c>searchTerm</c>, <c>isActive</c>, <c>pageNumber</c>, <c>pageSize</c>.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The requested page of products.</returns>
    /// <response code="200">The page was returned.</response>
    /// <response code="400">A query parameter was invalid.</response>
    /// <response code="401">No valid access token was presented.</response>
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
    /// Gets a single product by identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The requested product.</returns>
    /// <response code="200">The product was returned.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="404">No product with the given identifier exists.</response>
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
    /// Updates an existing product (stock is changed through separate stock movements, not here).
    /// Administrators only.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The product update payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated product.</returns>
    /// <response code="200">The product was updated.</response>
    /// <response code="400">The payload failed validation.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="403">The caller is not an administrator.</response>
    /// <response code="404">No product with the given identifier exists.</response>
    /// <response code="409">The SKU is already taken.</response>
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
    /// Deletes a product. Administrators only.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">The product was deleted.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="403">The caller is not an administrator.</response>
    /// <response code="404">No product with the given identifier exists.</response>
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
