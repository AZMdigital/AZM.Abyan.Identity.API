using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Entities.Base;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Persistence.DbContexts;

public class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options,
    ICurrentUserService? currentUserService = null) : DbContext(options)
{
    private readonly ICurrentUserService? _currentUserService = currentUserService;

    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<TenantUserRole> TenantUserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Scope> Scopes { get; set; }
    public DbSet<Policy> Policies { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<TenantUserPermission> TenantUserPermissions { get; set; }
    public DbSet<License> Licenses { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<LicenseClient> LicenseClients { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantUserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
        modelBuilder.ApplyConfiguration(new LicenseConfiguration());

        // Configure LicenseClient many-to-many relationship
        modelBuilder.Entity<LicenseClient>()
            .HasKey(lc => new { lc.LicenseId, lc.ClientId });

        modelBuilder.Entity<LicenseClient>()
            .HasOne(lc => lc.License)
            .WithMany(l => l.LicenseClients)
            .HasForeignKey(lc => lc.LicenseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LicenseClient>()
            .HasOne(lc => lc.Client)
            .WithMany(c => c.LicenseClients)
            .HasForeignKey(lc => lc.ClientId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Get userId from Claims and parse to Guid
        var currentUserId = _currentUserService?.GetCurrentUserId();


        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUserId ?? Guid.Empty;

                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId ?? Guid.Empty;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.SoftDelete(currentUserId);
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

}
