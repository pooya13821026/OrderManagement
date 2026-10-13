using System.Security.Claims;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Interfaces;

/// <summary>
/// Contract for JWT access token issuing and refresh token helpers.
/// The concrete JWT implementation lives in the Infrastructure layer.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a signed JWT access token for the given user.
    /// </summary>
    /// <param name="user">The user the token is issued for.</param>
    /// <returns>The signed JWT as a string.</returns>
    string GenerateAccessToken(ApplicationUser user);

    /// <summary>
    /// Generates a cryptographically random raw refresh token. Only its hash is stored.
    /// </summary>
    /// <returns>The raw refresh token.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Computes the hash of a raw refresh token for storage and lookup.
    /// </summary>
    /// <param name="token">The raw refresh token.</param>
    /// <returns>The token hash.</returns>
    string HashRefreshToken(string token);

    /// <summary>
    /// Builds the <see cref="ClaimsPrincipal"/> from an expired access token
    /// without validating its lifetime, so the principal can be used to look up
    /// the user during a refresh.
    /// </summary>
    /// <param name="token">The expired access token.</param>
    /// <returns>The principal extracted from the token.</returns>
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
