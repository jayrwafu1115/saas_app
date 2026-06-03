using Clinic.Application.Common.Interfaces;
using Clinic.Domain.Common;
using Clinic.Domain.Patients;
using Clinic.Domain.Tenants;
using Clinic.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Infrastructure.Persistence;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IApplicationDbContext
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientContact> PatientContacts => Set<PatientContact>();
    public DbSet<PatientDocument> PatientDocuments => Set<PatientDocument>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<ApplicationRolePermission> RolePermissions => Set<ApplicationRolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserActionToken> UserActionTokens => Set<UserActionToken>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditState();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    private void ApplyAuditState()
    {
        var now = dateTimeProvider.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedBy = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAtUtc = now;
                entry.Entity.ModifiedBy = userId;
            }
        }

        foreach (var entry in ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAtUtc = now;
            entry.Entity.DeletedBy = userId;
        }
    }
}
