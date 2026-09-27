using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SenaViva.Shared.Entities;

namespace SenaViva.API.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(user => user.Name).HasMaxLength(100).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(100).IsRequired();
        builder.Property(user => user.Auth0Id).HasMaxLength(128);
        builder.Property(user => user.Role).HasConversion<string>().HasMaxLength(45);

        builder.HasIndex(user => user.Email).IsUnique();
        builder.HasIndex(user => user.Auth0Id).IsUnique().HasFilter("[Auth0Id] IS NOT NULL");
    }
}
