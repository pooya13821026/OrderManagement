using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Security;

/// <summary>
/// Issues HMAC-SHA256 signed JWT access tokens and the refresh token helpers used by the
/// authentication service. Reads its settings from the <c>Jwt</c> configuration section
/// (<c>Key</c>, <c>Issuer</c>, <c>Audience</c>, <c>AccessTokenMinutes</c>); only the
/// signing key is mandatory.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    /// <summary>
    /// Access token lifetime used when <c>Jwt:AccessTokenMinutes</c> is missing or invalid.
    /// </summary>
    private const int DefaultAccessTokenMinutes = 15;

    /// <summary>
    /// Number of random bytes behind a raw refresh token (512 bits of entropy).
    /// </summary>
    private const int RefreshTokenByteCount = 64;

    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _key;
    private readonly int _accessTokenMinutes;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenService"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration; the <c>Jwt</c> section is read.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <c>Jwt:Key</c> is missing or blank.</exception>
    public JwtTokenService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        IConfigurationSection section = configuration.GetSection("Jwt");

        _key = section["Key"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_key))
        {
            // Never fall back to a built-in secret: a missing key is a configuration error.
            throw new InvalidOperationException("Missing required configuration value 'Jwt:Key'.");
        }

        _issuer = section["Issuer"] ?? "OrderManagement";
        _audience = section["Audience"] ?? "OrderManagement";

        _accessTokenMinutes = int.TryParse(section["AccessTokenMinutes"], out int minutes) && minutes > 0
            ? minutes
            : DefaultAccessTokenMinutes;
    }

    /// <inheritdoc />
    public string GenerateAccessToken(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Short, standard claim names: sub/email/given_name/family_name/jti are emitted
        // as-is; "role" is the conventional role claim read back by the API middleware.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("role", user.Role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        // Cryptographically random, unguessable token; only its hash is ever stored.
        byte[] randomBytes = RandomNumberGenerator.GetBytes(RefreshTokenByteCount);
        return Convert.ToBase64String(randomBytes);
    }

    /// <inheritdoc />
    public string HashRefreshToken(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);

        // SHA256 hex digest: fixed size, unique-index friendly, and safe to persist even
        // though the token itself already has high entropy.
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    /// <inheritdoc />
    /// <exception cref="SecurityTokenException">Thrown when the signature, issuer or audience is invalid.</exception>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            // The whole point of a refresh: an expired access token must still be readable,
            // but only its signature proves it really was issued by this server.
            ValidateLifetime = false,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, parameters, out _);
    }
}
