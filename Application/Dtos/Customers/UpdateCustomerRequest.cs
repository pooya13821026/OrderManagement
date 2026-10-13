using System.ComponentModel.DataAnnotations;

namespace OrderManagement.Application.Dtos.Customers;

/// <summary>
/// Payload for updating an existing customer.
/// </summary>
public class UpdateCustomerRequest
{
    /// <summary>
    /// Gets or sets the first name of the customer.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the customer.
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the customer. Must stay unique.
    /// </summary>
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional phone number of the customer.
    /// </summary>
    [Phone]
    [MaxLength(20)]
    public string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the optional postal address of the customer.
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }
}
