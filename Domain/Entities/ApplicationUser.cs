namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents an application user account used for authentication and authorization.
/// </summary>
public class ApplicationUser
{
    /// <summary>
    /// Unique identifier of the user account.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique login name of the user.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the user. Unique across all accounts.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password of the user. Never store or expose the plain text password.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// First name of the user.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the user.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Optional phone number of the user.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Indicates whether the account is active and may sign in. Defaults to <c>true</c>.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Role of the user (for example <c>User</c> or <c>Admin</c>). Defaults to <c>User</c>.
    /// </summary>
    public string Role { get; set; } = "User";

    /// <summary>
    /// UTC timestamp when the account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp of the last successful sign in, or <c>null</c> if the user never signed in.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Security stamp used to invalidate tokens when the account changes.
    /// </summary>
    public string SecurityStamp { get; set; } = string.Empty;

    /// <summary>
    /// Concurrency token managed by Entity Framework. A mismatch indicates a concurrent modification.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
