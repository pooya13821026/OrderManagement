using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Auth;

/// <summary>
/// Payload for creating a new user account.
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Gets or sets the email address of the new account. Unique.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the plain text password. Hashed before storage; never persisted as-is.
    /// </summary>
    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional phone number of the user.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
