namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents a stored refresh token. Only the hash of the token is persisted;
/// the raw token is returned to the client exactly once at issuance.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Unique identifier of the stored refresh token record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the user the token belongs to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Hash of the issued refresh token. Unique across all records.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// UTC moment after which the token is no longer valid.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// UTC moment when the token record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC moment when the token was revoked, or <c>null</c> while it is still active.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Hash of the token that replaced this one during rotation, or <c>null</c> if not rotated.
    /// </summary>
    public string? ReplacedByTokenHash { get; set; }

    /// <summary>
    /// Gets a value indicating whether the token is currently usable
    /// (never revoked and not yet expired).
    /// </summary>
    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Navigation property to the owning user account.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;
}
