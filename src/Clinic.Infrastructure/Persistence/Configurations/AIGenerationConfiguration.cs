using Clinic.Domain.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public sealed class AIGenerationConfiguration : IEntityTypeConfiguration<AIGeneration>
{
    public void Configure(EntityTypeBuilder<AIGeneration> builder)
    {
        builder.ToTable("AIGenerations");
        builder.HasKey(generation => generation.Id);
        builder.Property(generation => generation.Type).HasConversion<string>().HasMaxLength(80).IsRequired();
        builder.Property(generation => generation.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(generation => generation.Provider).HasMaxLength(40).IsRequired();
        builder.Property(generation => generation.Model).HasMaxLength(120).IsRequired();
        builder.Property(generation => generation.PromptHash).HasMaxLength(128).IsRequired();
        builder.Property(generation => generation.Prompt).HasMaxLength(8000).IsRequired();
        builder.Property(generation => generation.Output).HasMaxLength(16000).IsRequired();
        builder.Property(generation => generation.ErrorMessage).HasMaxLength(2000).IsRequired();
        builder.Property(generation => generation.RequestedBy).HasMaxLength(80).IsRequired();
        builder.Property(generation => generation.CostUsd).HasPrecision(18, 6);
        builder.HasQueryFilter(generation => !generation.IsDeleted);
        builder.HasIndex(generation => new { generation.TenantId, generation.EncounterId, generation.CreatedAtUtc });
        builder.HasIndex(generation => new { generation.Provider, generation.Type, generation.PromptHash });
    }
}
