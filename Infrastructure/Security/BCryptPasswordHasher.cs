using OrderManagement.Application.Interfaces;

namespace OrderManagement.Infrastructure.Security;

/// <summary>
/// One-way password hashing with BCrypt, implementing <see cref="IPasswordHasher"/>.
/// BCrypt embeds a per-password salt in the hash, so identical passwords never
/// produce identical hashes.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    /// <summary>
    /// BCrypt work factor: 11 keeps hashing comfortably above interactive brute-force
    /// speeds while staying well under the latency budget of a sign-in request.
    /// </summary>
    private const int WorkFactor = 11;

    /// <inheritdoc />
    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <inheritdoc />
    public bool Verify(string password, string passwordHash)
    {
        // An empty input can never match a stored hash; returning false keeps the
        // caller's "invalid credentials" path without leaking why it failed.
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // A malformed stored hash is treated as a mismatch instead of crashing sign-in.
            return false;
        }
    }
}
