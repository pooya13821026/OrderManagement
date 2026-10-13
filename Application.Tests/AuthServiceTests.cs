using Moq;
using OrderManagement.Application.Dtos.Auth;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Services;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Exceptions;
using OrderManagement.Domain.Repositories;
using Xunit;

namespace OrderManagement.Application.Tests;

/// <summary>
/// تست‌های واحد <see cref="AuthService"/> با شبیه‌سازی واحد کاری، سرویس توکن و هش گذرواژه؛
/// بدون EF Core، بدون پایگاه داده و بدون شبکه.
/// </summary>
public class AuthServiceTests
{
    private const string StoredPasswordHash = "stored-hash";

    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly AuthService _service;

    /// <summary>
    /// نمونه‌ای جدید می‌سازد و توکن‌ها و هش‌ها را با مقادیر قطعی شبیه‌سازی می‌کند.
    /// </summary>
    public AuthServiceTests()
    {
        _unitOfWork.SetupGet(u => u.Users).Returns(_userRepository.Object);
        _unitOfWork.SetupGet(u => u.RefreshTokens).Returns(_refreshTokenRepository.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _tokenService.SetupGet(t => t.AccessTokenLifetimeMinutes).Returns(60);
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<ApplicationUser>())).Returns("access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("raw-refresh");
        _tokenService.Setup(t => t.HashRefreshToken(It.IsAny<string>())).Returns<string>(token => "hash:" + token);

        _service = new AuthService(_unitOfWork.Object, _tokenService.Object, _passwordHasher.Object);
    }

    /// <summary>
    /// ثبت‌نام با ایمیل تازه باید گذرواژه را هش کند، کاربر را با نقش پیش‌فرض User اضافه کند و جفت توکن برگرداند.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_NewEmail_ReturnsTokensAndDefaultUserRole()
    {
        const string email = "new@example.com";
        const string password = "Password123!";
        _userRepository.Setup(r => r.ExistsByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(password)).Returns("hashed");

        AuthResponse response = await _service.RegisterAsync(new RegisterRequest
        {
            Email = email,
            Password = password,
            FirstName = "Ali",
            LastName = "Rezaei"
        });

        Assert.Equal("access", response.AccessToken);
        Assert.Equal("raw-refresh", response.RefreshToken);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
        Assert.Equal(email, response.User.Email);
        Assert.Equal(Roles.User, response.User.Role);
        _passwordHasher.Verify(h => h.Hash(password), Times.Once);
        _userRepository.Verify(
            r => r.AddAsync(
                It.Is<ApplicationUser>(u => u.Email == email && u.PasswordHash == "hashed" && u.Role == Roles.User),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _refreshTokenRepository.Verify(
            r => r.AddAsync(It.Is<RefreshToken>(t => t.TokenHash == "hash:raw-refresh"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// ثبت‌نام با ایمیل تکراری باید DuplicateEntityException پرتاب کند و کاربری اضافه نشود.
    /// </summary>
    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsDuplicate()
    {
        const string email = "dup@example.com";
        _userRepository.Setup(r => r.ExistsByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var request = new RegisterRequest
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Ali",
            LastName = "Rezaei"
        };

        await Assert.ThrowsAsync<DuplicateEntityException>(() => _service.RegisterAsync(request));

        _userRepository.Verify(r => r.AddAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()), Times.Never);
        _passwordHasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// ورود با گذرواژه‌ی اشتباه باید خطای عمومی UnauthorizedOperationException بدهد و جزئیاتی از وجود حساب فاش نکند.
    /// </summary>
    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        const string email = "user@example.com";
        const string wrongPassword = "WrongPassword";
        ApplicationUser user = CreateUser(email);
        _userRepository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify(wrongPassword, StoredPasswordHash)).Returns(false);

        UnauthorizedOperationException exception = await Assert.ThrowsAsync<UnauthorizedOperationException>(
            () => _service.LoginAsync(new LoginRequest { Email = email, Password = wrongPassword }));

        Assert.Equal("ایمیل یا گذرواژه نامعتبر است.", exception.Message);
        Assert.DoesNotContain(email, exception.Message);
        _userRepository.Verify(r => r.Update(It.IsAny<ApplicationUser>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// ورود به حساب غیرفعال با گذرواژه‌ی درست باید UnauthorizedOperationException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorized()
    {
        const string email = "inactive@example.com";
        ApplicationUser user = CreateUser(email, isActive: false);
        _userRepository.Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("Password123!", StoredPasswordHash)).Returns(true);

        UnauthorizedOperationException exception = await Assert.ThrowsAsync<UnauthorizedOperationException>(
            () => _service.LoginAsync(new LoginRequest { Email = email, Password = "Password123!" }));

        Assert.Equal("این حساب غیرفعال شده است.", exception.Message);
        _userRepository.Verify(r => r.Update(It.IsAny<ApplicationUser>()), Times.Never);
    }

    /// <summary>
    /// refresh با توکن معتبر باید چرخه‌ی توکن انجام دهد: توکن قدیمی باطل و به توکن جدید زنجیر شود، توکن جدید ذخیره و برگردانده شود.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ValidToken_Rotates()
    {
        const string presentedToken = "old-refresh";
        ApplicationUser user = CreateUser("rotate@example.com");
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "hash:old-refresh",
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            RevokedAt = null,
            User = user
        };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash:old-refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        AuthResponse response = await _service.RefreshAsync(new RefreshRequest { RefreshToken = presentedToken });

        Assert.True(storedToken.RevokedAt.HasValue);
        Assert.Equal("hash:raw-refresh", storedToken.ReplacedByTokenHash);
        _refreshTokenRepository.Verify(r => r.Update(storedToken), Times.Once);
        _refreshTokenRepository.Verify(
            r => r.AddAsync(
                It.Is<RefreshToken>(t => t.TokenHash == "hash:raw-refresh" && t.UserId == user.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("raw-refresh", response.RefreshToken);
        Assert.Equal("access", response.AccessToken);
        Assert.Equal(user.Id, response.User.Id);
    }

    /// <summary>
    /// refresh با توکن باطل‌شده باید UnauthorizedOperationException پرتاب کند و توکن جدیدی صادر نشود.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_RevokedToken_ThrowsUnauthorized()
    {
        const string presentedToken = "revoked-refresh";
        ApplicationUser user = CreateUser("revoked@example.com");
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "hash:revoked-refresh",
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            RevokedAt = DateTime.UtcNow.AddDays(-1),
            User = user
        };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash:revoked-refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        await Assert.ThrowsAsync<UnauthorizedOperationException>(
            () => _service.RefreshAsync(new RefreshRequest { RefreshToken = presentedToken }));

        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Never);
    }

    /// <summary>
    /// refresh با توکن منقضی‌شده باید UnauthorizedOperationException پرتاب کند و توکن جدیدی صادر نشود.
    /// </summary>
    [Fact]
    public async Task RefreshAsync_ExpiredToken_ThrowsUnauthorized()
    {
        const string presentedToken = "expired-refresh";
        ApplicationUser user = CreateUser("expired@example.com");
        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "hash:expired-refresh",
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            RevokedAt = null,
            User = user
        };
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash:expired-refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        await Assert.ThrowsAsync<UnauthorizedOperationException>(
            () => _service.RefreshAsync(new RefreshRequest { RefreshToken = presentedToken }));

        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Never);
    }

    /// <summary>
    /// باطل کردن توکن ناشناخته باید EntityNotFoundException پرتاب کند.
    /// </summary>
    [Fact]
    public async Task RevokeAsync_UnknownToken_ThrowsEntityNotFound()
    {
        _refreshTokenRepository.Setup(r => r.GetByTokenHashAsync("hash:missing-refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.RevokeAsync("missing-refresh"));

        _refreshTokenRepository.Verify(r => r.Update(It.IsAny<RefreshToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// یک کاربر ساده برای تست‌ها می‌سازد.
    /// </summary>
    private static ApplicationUser CreateUser(string email, bool isActive = true)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "tester",
            Email = email,
            PasswordHash = StoredPasswordHash,
            FirstName = "Test",
            LastName = "User",
            IsActive = isActive,
            Role = Roles.User,
            CreatedAt = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };
    }
}
