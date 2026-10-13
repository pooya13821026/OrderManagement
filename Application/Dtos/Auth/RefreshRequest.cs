using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Auth;

/// <summary>
/// Payload for exchanging a valid refresh token for a new token pair (rotation).
/// </summary>
public class RefreshRequest
{
    /// <summary>
    /// Gets or sets the refresh token previously issued to the client.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string RefreshToken { get; set; } = string.Empty;
}
