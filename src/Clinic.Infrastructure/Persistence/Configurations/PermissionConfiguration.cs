using Clinic.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(permission => permission.Id);

        builder.Property(permission => permission.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(permission => permission.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasIndex(permission => permission.Name)
            .IsUnique();
    }
}
