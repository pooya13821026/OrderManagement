using OrderManagement.Application.Dtos.Auth;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// Application service contract for authentication: registration, sign-in,
/// refresh token rotation and revocation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user with the default <c>User</c> role, hashing the password,
    /// and returns a fresh token pair.
    /// </summary>
    /// <param name="request">The registration payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The authentication response with tokens and the user profile.</returns>
    /// <exception cref="Domain.Exceptions.DuplicateEntityException">Thrown when the email is already registered.</exception>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Signs in with email and password, updates the last-login timestamp
    /// and returns a fresh token pair.
    /// </summary>
    /// <param name="request">The login payload.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The authentication response with tokens and the user profile.</returns>
    /// <exception cref="Domain.Exceptions.UnauthorizedOperationException">
    /// Thrown when credentials are wrong or the account is disabled.
    /// </exception>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a valid, unrevoked, unexpired refresh token for a new token pair,
    /// revoking the old token (rotation).
    /// </summary>
    /// <param name="request">The refresh payload containing the current refresh token.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The authentication response with the new tokens and the user profile.</returns>
    Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes the given refresh token so it can no longer be exchanged.
    /// </summary>
    /// <param name="refreshToken">The raw refresh token to revoke.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Thrown when no stored token matches.</exception>
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the profile of the currently authenticated user.
    /// </summary>
    /// <param name="userId">The identifier of the current user.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The user profile as a DTO.</returns>
    /// <exception cref="Domain.Exceptions.EntityNotFoundException">Thrown when the user does not exist.</exception>
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
