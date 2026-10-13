namespace OrderManagement.Application.Dtos.Auth;

/// <summary>
/// Result of a successful authentication operation (register, login or refresh).
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// Gets or sets the signed JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw refresh token. Only its hash is stored server side.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC expiry moment of the access token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the authenticated user's profile information.
    /// </summary>
    public UserDto User { get; set; } = null!;
}
