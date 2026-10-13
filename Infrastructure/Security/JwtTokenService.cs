using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application.Interfaces;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Security;

/// <summary>
/// توکن‌های دسترسی JWT امضاشده با HMAC-SHA256 و کمک‌کننده‌های توکن refresh مورد استفاده‌ی
/// سرویس احراز هویت را صادر می‌کند. تنظیمات خود را از بخش پیکربندی <c>Jwt</c>
/// (<c>Key</c>، <c>Issuer</c>، <c>Audience</c>، <c>AccessTokenMinutes</c>) می‌خواند؛ فقط
/// کلید امضا الزامی است.
/// </summary>
public sealed class JwtTokenService : ITokenService
{
    /// <summary>
    /// طول عمر توکن دسترسی که وقتی <c>Jwt:AccessTokenMinutes</c> غایب یا نامعتبر باشد استفاده می‌شود.
    /// </summary>
    private const int DefaultAccessTokenMinutes = 15;

    /// <summary>
    /// تعداد بایت‌های تصادفی پشت یک توکن refresh خام (۵۱۲ بیت آنتروپی).
    /// </summary>
    private const int RefreshTokenByteCount = 64;

    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _key;
    private readonly int _accessTokenMinutes;

    /// <summary>
    /// نمونه‌ای جدید از کلاس <see cref="JwtTokenService"/> می‌سازد.
    /// </summary>
    /// <param name="configuration">پیکربندی برنامه؛ بخش <c>Jwt</c> خوانده می‌شود.</param>
    /// <exception cref="ArgumentNullException">وقتی <paramref name="configuration"/> برابر <c>null</c> باشد پرتاب می‌شود.</exception>
    /// <exception cref="InvalidOperationException">وقتی <c>Jwt:Key</c> غایب یا خالی باشد پرتاب می‌شود.</exception>
    public JwtTokenService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        IConfigurationSection section = configuration.GetSection("Jwt");

        _key = section["Key"] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_key))
        {
            // هرگز به یک secret داخلی برنگردید: نبودن کلید یک خطای پیکربندی است.
            throw new InvalidOperationException("مقدار پیکربندی الزامی 'Jwt:Key' وجود ندارد.");
        }

        _issuer = section["Issuer"] ?? "OrderManagement";
        _audience = section["Audience"] ?? "OrderManagement";

        _accessTokenMinutes = int.TryParse(section["AccessTokenMinutes"], out int minutes) && minutes > 0
            ? minutes
            : DefaultAccessTokenMinutes;
    }

    /// <inheritdoc />
    public string GenerateAccessToken(ApplicationUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // نام‌های claim کوتاه و استاندارد: sub/email/given_name/family_name/jti همان
        // طور که هستند منتشر می‌شوند؛ "role" claim رسمی نقش است که middleware های API می‌خوانند.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("role", user.Role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        // توکنی تصادفی در سطح رمزنگاری و غیرقابل حدس؛ فقط هش آن ذخیره می‌شود.
        byte[] randomBytes = RandomNumberGenerator.GetBytes(RefreshTokenByteCount);
        return Convert.ToBase64String(randomBytes);
    }

    /// <inheritdoc />
    public string HashRefreshToken(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);

        // دیجست hex از SHA256: اندازه‌ی ثابت، سازگار با ایندکس یکتا، و امن برای ذخیره‌سازی حتی
        // با اینکه خودِ توکن آنتروپی بالایی دارد.
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    /// <inheritdoc />
    /// <exception cref="SecurityTokenException">وقتی امضا، issuer یا audience نامعتبر باشد پرتاب می‌شود.</exception>
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        ArgumentException.ThrowIfNullOrEmpty(token);

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            // نکته‌ی اصلی refresh: یک توکن دسترسی منقضی‌شده باید همچنان قابل خواندن باشد،
            // اما فقط امضای آن ثابت می‌کند که واقعاً توسط همین سرور صادر شده است.
            ValidateLifetime = false,
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ClockSkew = TimeSpan.Zero
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, parameters, out _);
    }
}
