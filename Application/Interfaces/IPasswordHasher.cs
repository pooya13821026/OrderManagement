namespace OrderManagement.Application.Interfaces;

/// <summary>
/// Contract for one-way password hashing and verification.
/// The concrete algorithm (for example BCrypt) is provided by the Infrastructure layer.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain text password.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The password hash to persist.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain text password against a previously computed hash.
    /// </summary>
    /// <param name="password">The plain text password to check.</param>
    /// <param name="passwordHash">The stored password hash.</param>
    /// <returns><c>true</c> when the password matches the hash; otherwise <c>false</c>.</returns>
    bool Verify(string password, string passwordHash);
}
