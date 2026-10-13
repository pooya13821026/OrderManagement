using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps <see cref="RefreshToken"/> to the <c>RefreshTokens</c> table: only the token hash
/// is persisted (unique), tokens are deleted together with their user and the computed
/// active flag is never stored.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);

        builder.Property(token => token.UserId).IsRequired();
        // The SHA256 hex digest is what gets stored and looked up; the raw token
        // is returned to the client exactly once and never persisted.
        builder.Property(token => token.TokenHash).IsRequired().HasMaxLength(128);
        builder.Property(token => token.ExpiresAt).IsRequired();
        builder.Property(token => token.CreatedAt).IsRequired();
        builder.Property(token => token.ReplacedByTokenHash).HasMaxLength(128);

        // Lookups by hash drive refresh and revocation, and no two tokens may share a hash.
        builder.HasIndex(token => token.TokenHash).IsUnique();

        // IsActive is derived from RevokedAt/ExpiresAt; storing it would allow drift.
        builder.Ignore(token => token.IsActive);

        // Refresh tokens are meaningless without their account: remove them together.
        builder.HasOne(token => token.User)
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
