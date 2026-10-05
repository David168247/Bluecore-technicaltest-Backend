using BluecoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BluecoreApi.Data.Configurations;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> entity)
    {
        entity.ToTable("user_accounts", "esquema_c");
        entity.HasKey(x => x.Id).HasName("pk_user_accounts");
        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
        entity.Property(x => x.NormalizedUsername).HasColumnName("normalized_username").HasMaxLength(50).IsRequired();
        entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        entity.Property(x => x.NormalizedEmail).HasColumnName("normalized_email").HasMaxLength(254).IsRequired();
        entity.Property(x => x.PasswordHash).HasColumnName("password_hash").IsRequired();
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");
        entity.HasIndex(x => x.NormalizedUsername).IsUnique().HasDatabaseName("ux_user_accounts_normalized_username");
        entity.HasIndex(x => x.NormalizedEmail).IsUnique().HasDatabaseName("ux_user_accounts_normalized_email");
    }
}
