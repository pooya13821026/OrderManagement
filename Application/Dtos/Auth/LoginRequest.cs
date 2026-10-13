using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Auth;

/// <summary>
/// Payload for signing in with email and password.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the email address of the account.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plain text password to verify.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
