using OrderManagement.Application.Dtos.Auth;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Mapping;
using OrderManagement.Application.Validators;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exceptions;

namespace OrderManagement.Application.Services;

/// <summary>
/// پیاده‌سازی احراز هویت: ثبت‌نام، ورود، چرخه‌ی توکن refresh و ابطال آن.
/// گذرواژه‌ها از طریق <see cref="IPasswordHasher"/> هش و JWTها از طریق
/// <see cref="ITokenService"/> صادر می‌شوند؛ فقط هش توکن‌های refresh ذخیره می‌شوند.
/// </summary>
public class AuthService : IAuthService
{
    /// <summary>
    /// طول عمر توکن‌های refresh تازه صادرشده.
    /// </summary>
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="AuthService"/> می‌سازد.
    /// </summary>
    /// <param name="unitOfWork">واحد کاری که برای همه‌ی ذخیره‌سازی‌ها استفاده می‌شود.</param>
    /// <param name="tokenService">سرویس صادرکننده‌ی توکن‌های دسترسی و refresh.</param>
    /// <param name="passwordHasher">سرویس هش و تأیید گذرواژه‌ها.</param>
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

        // نگهبان تکراری به‌صورت best-effort؛ ایندکس یکتای دیتابیس همچنان حافظ
        // مقتدرانه در برابر ثبت‌نام‌های همزمان است.
        if (await _unitOfWork.Users.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new DuplicateEntityException(nameof(ApplicationUser), $"کاربری با ایمیل '{normalizedEmail}' از قبل وجود دارد.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            // پیشوند ایمیل نام کاربری را برای این سرویس به‌قدر کافی یکتا نگه می‌دارد؛
            // خودِ یکتایی همچنان توسط دیتابیس تضمین می‌شود.
            UserName = BuildUserName(normalizedEmail),
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            IsActive = true,
            // ثبت‌نام همیشه کاربر عادی می‌سازد؛ مدیران در جای دیگری seed یا ارتقا داده می‌شوند.
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

        // یک خطای عمومی برای ایمیل ناشناخته و گذرواژه‌ی اشتباه، از فاش شدن
        // اینکه کدام حساب‌ها وجود دارند جلوگیری می‌کند (user enumeration).
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedOperationException("ایمیل یا گذرواژه نامعتبر است.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedOperationException("این حساب غیرفعال شده است.");
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
            ?? throw new UnauthorizedOperationException("توکن refresh معتبر نیست.");

        if (storedToken.RevokedAt is not null)
        {
            throw new UnauthorizedOperationException("توکن refresh باطل شده است.");
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedOperationException("توکن refresh منقضی شده است.");
        }

        ApplicationUser user = storedToken.User
            ?? throw new UnauthorizedOperationException("توکن refresh معتبر نیست.");

        if (!user.IsActive)
        {
            throw new UnauthorizedOperationException("این حساب غیرفعال شده است.");
        }

        // چرخه (rotation): باطل کردن توکن ارائه‌شده و صدور توکن جایگزین در همان ذخیره‌سازی.
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
            ?? throw new EntityNotFoundException(nameof(RefreshToken), "توکن ارائه‌شده");

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
    /// جفت توکن دسترسی و refresh جدیدی برای کاربر صادر می‌کند و هش توکن refresh را ذخیره
    /// می‌کند تا هنگام refresh قابل اعتبارسنجی باشد.
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
    /// نام کاربری را از بخش محلی ایمیل می‌سازد؛ کاراکترهای غیرحرفی حذف می‌شوند
    /// و یک پسوند تصادفی کوتاه برخورد نام‌ها را بعید می‌کند.
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
