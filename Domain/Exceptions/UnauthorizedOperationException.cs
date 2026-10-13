namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// Thrown when a caller is not permitted to perform the requested operation,
/// for example when a non-administrator tries to cancel an order or when credentials are invalid.
/// </summary>
public class UnauthorizedOperationException : DomainException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedOperationException"/> class.
    /// </summary>
    public UnauthorizedOperationException()
        : base("The operation is not authorized.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedOperationException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public UnauthorizedOperationException(string message)
        : base(message)
    {
    }
}
