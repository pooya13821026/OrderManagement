namespace OrderManagement.Domain.Entities;

/// <summary>
/// Represents a customer of the store. Email is unique across all customers.
/// </summary>
public class Customer
{
    /// <summary>
    /// Unique identifier of the customer.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// First name of the customer.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the customer.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the customer. Unique, used also for lookups and duplicate checks.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional phone number of the customer.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Optional postal address of the customer.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// UTC timestamp when the customer record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp of the last update of the customer record.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Concurrency token managed by Entity Framework. A mismatch indicates a concurrent modification.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
