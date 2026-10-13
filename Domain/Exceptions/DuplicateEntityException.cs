namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// Thrown when creating or updating an entity would violate a uniqueness constraint,
/// for example a duplicate email address or SKU.
/// </summary>
public class DuplicateEntityException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEntityException"/> class
    /// with a default message for the given entity.
    /// </summary>
    /// <param name="entityName">Simple name of the entity type, for example <c>Customer</c>.</param>
    public DuplicateEntityException(string entityName)
        : base($"{entityName} already exists.")
    {
        EntityName = entityName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateEntityException"/> class
    /// with a custom message.
    /// </summary>
    /// <param name="entityName">Simple name of the entity type, for example <c>Customer</c>.</param>
    /// <param name="message">The message that describes the error.</param>
    public DuplicateEntityException(string entityName, string message)
        : base(message)
    {
        EntityName = entityName;
    }

    /// <summary>
    /// Gets the simple name of the entity type that caused the conflict.
    /// </summary>
    public string EntityName { get; }
}
