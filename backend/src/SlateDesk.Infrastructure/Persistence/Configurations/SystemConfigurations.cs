using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SlateDesk.Domain.Entities;
using SlateDesk.Infrastructure.Identity;

namespace SlateDesk.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(token => token.Id);

        builder.Property(token => token.UserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(token => token.TokenHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(token => token.RevokedReason)
            .HasMaxLength(250);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(token => token.ParentTokenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(token => token.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(token => token.TokenHash)
            .IsUnique();

        builder.HasIndex(token => new
        {
            token.FamilyId,
            token.RevokedAtUtc
        });
    }
}

internal sealed class AuditLogConfiguration
    : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(auditLog => auditLog.Id);

        builder.Property(auditLog => auditLog.UserId)
            .HasMaxLength(450);

        builder.Property(auditLog => auditLog.Action)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(auditLog => auditLog.EntityType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(auditLog => auditLog.EntityId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(auditLog => auditLog.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(auditLog => auditLog.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(auditLog => new
        {
            auditLog.EntityType,
            auditLog.EntityId,
            auditLog.CreatedAtUtc
        });
    }
}

internal sealed class AppSettingConfiguration
    : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("AppSettings");

        builder.HasKey(setting => setting.Id);

        builder.Property(setting => setting.Key)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(setting => setting.Value)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(setting => setting.Description)
            .HasMaxLength(500);

        builder.Property(setting => setting.UpdatedByUserId)
            .HasMaxLength(450);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(setting => setting.UpdatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(setting => setting.Key)
            .IsUnique();
    }
}