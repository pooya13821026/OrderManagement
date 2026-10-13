using OrderManagement.Application.Dtos.Auth;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mapping;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Application.Services;

/// <summary>
/// Implements authentication: registration, sign-in, refresh token rotation and revocation.
/// Passwords are hashed through <see cref="IPasswordHasher"/> and JWTs are issued through
/// <see cref="ITokenService"/>; only refresh token hashes are persisted.
/// </summary>
public class AuthService : IAuthService
{
    /// <summary>
    /// Lifetime of newly issued refresh tokens.
    /// </summary>
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work used for all persistence.</param>
    /// <param name="tokenService">The service issuing access and refresh tokens.</param>
    /// <param name="passwordHasher">The service hashing and verifying passwords.</param>
    public AuthService(IUnitOfWork unitOfWork, ITokenService tokenService, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    /// <inheritdoc />
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.EnsureNotEmpty(request.Email, nameof(request.Email));
        RequestValidator.EnsureNotEmpty(request.Password, nameof(request.Password));

        string normalizedEmail = request.Email.Trim();

        // Best-effort duplicate guard; the unique index in the database remains the
        // authoritative protection against concurrent registrations.
        if (await _unitOfWork.Users.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(ApplicationUser), $"A user with email '{normalizedEmail}' already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            // The email prefix keeps the user name unique enough for this service;
            // uniqueness itself is still enforced by the database.
            UserName = BuildUserName(normalizedEmail),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true,
            // Registration always creates a regular user; administrators are seeded/promoted elsewhere.
            Role = Roles.User,
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.EnsureNotEmpty(request.Email, nameof(request.Email));
        RequestValidator.EnsureNotEmpty(request.Password, nameof(request.Password));

        string normalizedEmail = request.Email.Trim();
        ApplicationUser? user = await _unitOfWork.Users.GetByEmailAsync(normalizedEmail, cancellationToken);

        // One generic error for unknown email and wrong password avoids revealing
        // which accounts exist (user enumeration).
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedOperationException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedOperationException("This account has been disabled.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequestValidator.EnsureNotEmpty(request.RefreshToken, nameof(request.RefreshToken));

        string tokenHash = _tokenService.HashRefreshToken(request.RefreshToken.Trim());
        RefreshToken storedToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedOperationException("The refresh token is not valid.");

        if (storedToken.RevokedAt is not null)
        {
            throw new UnauthorizedOperationException("The refresh token has been revoked.");
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedOperationException("The refresh token has expired.");
        }

        ApplicationUser user = storedToken.User
            ?? throw new UnauthorizedOperationException("The refresh token is not valid.");

        if (!user.IsActive)
        {
            throw new UnauthorizedOperationException("This account has been disabled.");
        }

        // Rotation: revoke the presented token and issue a replacement in the same save.
        string newRawToken = _tokenService.GenerateRefreshToken();
        string newTokenHash = _tokenService.HashRefreshToken(newRawToken);
        DateTime now = DateTime.UtcNow;

        storedToken.RevokedAt = now;
        storedToken.ReplacedByTokenHash = newTokenHash;
        _unitOfWork.RefreshTokens.Update(storedToken);

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newTokenHash,
            ExpiresAt = now.Add(RefreshTokenLifetime),
            CreatedAt = now,
            User = user
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = newRawToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = user.ToDto()
        };
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmpty(refreshToken, nameof(refreshToken));

        string tokenHash = _tokenService.HashRefreshToken(refreshToken.Trim());
        RefreshToken storedToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(RefreshToken), "the provided token");

        if (storedToken.RevokedAt is null)
        {
            storedToken.RevokedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(storedToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        RequestValidator.EnsureNotEmptyId(userId, nameof(userId));

        ApplicationUser user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(ApplicationUser), userId);

        return user.ToDto();
    }

    /// <summary>
    /// Issues a new access token and refresh token pair for the user and persists the
    /// refresh token hash so it can be validated during refresh.
    /// </summary>
    private async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        string rawRefreshToken = _tokenService.GenerateRefreshToken();
        string tokenHash = _tokenService.HashRefreshToken(rawRefreshToken);

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.Add(RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow,
            User = user
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = _tokenService.GenerateAccessToken(user),
            RefreshToken = rawRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = user.ToDto()
        };
    }

    /// <summary>
    /// Builds a user name from the email local part; non-letter characters are stripped
    /// and a short random suffix keeps collisions unlikely.
    /// </summary>
    private static string BuildUserName(string email)
    {
        string localPart = email.Split('@')[0];
        string sanitized = new string(localPart.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        if (string.IsNullOrEmpty(sanitized))
        {
            sanitized = "user";
        }

        return $"{sanitized}-{Guid.NewGuid().ToString("N")[..8]}";
    }
}
