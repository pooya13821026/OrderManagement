using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Dtos.Auth;
using OrderManagement.Application.Interfaces;

namespace OrderManagement.Api.Controllers;

/// <summary>
/// Authentication endpoints: registration, sign-in, refresh token rotation, revocation
/// and the current user's profile.
/// </summary>
/// <remarks>
/// The controller requires authentication by default; registration, login and refresh are
/// explicitly anonymous because a client needs them to obtain (or renew) a token in the
/// first place. Revocation and the profile endpoint require a valid access token.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="authService">The authentication service.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="authService"/> is <c>null</c>.</exception>
    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// Registers a new user account with the default <c>User</c> role and returns
    /// the freshly issued access/refresh token pair.
    /// </summary>
    /// <param name="request">The registration payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created user together with the issued tokens.</returns>
    /// <response code="201">The account was created.</response>
    /// <response code="400">The payload failed validation.</response>
    /// <response code="409">The email address is already registered.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        AuthResponse response = await _authService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCurrentUser), response);
    }

    /// <summary>
    /// Signs in with email and password and returns a fresh token pair.
    /// </summary>
    /// <param name="request">The login payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The issued tokens and the user profile.</returns>
    /// <response code="200">The credentials were accepted.</response>
    /// <response code="400">The payload failed validation.</response>
    /// <response code="403">The credentials are wrong or the account is disabled.</response>
    [HttpPost("login")]
    [AllowAnonymous]
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
    /// Exchanges a valid, unrevoked refresh token for a new token pair (rotation).
    /// </summary>
    /// <param name="request">The refresh token to exchange.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The new tokens and the user profile.</returns>
    /// <response code="200">The token was rotated.</response>
    /// <response code="400">The payload failed validation or the refresh token is no longer usable.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
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
    /// Revokes the given refresh token so it can no longer be exchanged for new tokens.
    /// Requires an authenticated caller (sign-out / token clean-up).
    /// </summary>
    /// <param name="request">The refresh token to revoke.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">The token was revoked.</response>
    /// <response code="400">The payload failed validation.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="404">No stored refresh token matches the supplied value.</response>
    [HttpPost("revoke")]
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
    /// Gets the profile of the user represented by the presented access token.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The current user's profile.</returns>
    /// <response code="200">The profile was returned.</response>
    /// <response code="401">No valid access token was presented.</response>
    /// <response code="404">The account behind the token no longer exists.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
    {
        // The JWT "sub" claim arrives mapped as ClaimTypes.NameIdentifier (inbound claim
        // mapping configured with the bearer middleware).
        string? subject = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(subject, out Guid userId))
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid access token",
                detail: "The access token does not contain a usable user identifier.");
        }

        UserDto user = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        return Ok(user);
    }
}
