namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// Thrown when an entity with a given identifier does not exist.
/// </summary>
public class EntityNotFoundException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
    /// </summary>
    /// <param name="entityName">Simple name of the entity type, for example <c>Order</c>.</param>
    /// <param name="id">The identifier that could not be found.</param>
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found.")
    {
        EntityName = entityName;
        Id = id;
    }

    /// <summary>
    /// Gets the simple name of the entity type that was not found.
    /// </summary>
    public string EntityName { get; }

    /// <summary>
    /// Gets the identifier that could not be found.
    /// </summary>
    public object Id { get; }
}
