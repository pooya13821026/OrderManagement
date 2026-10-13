namespace OrderManagement.Domain.Exceptions;

/// <summary>
/// Base exception for all business rule violations raised by the domain or application layers.
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public DomainException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that caused this exception.</param>
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
