namespace OrderManagement.Application.Dtos.Customers;

/// <summary>
/// Public projection of a customer.
/// </summary>
public class CustomerDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the customer.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the optional postal address.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the customer was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the last update.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
