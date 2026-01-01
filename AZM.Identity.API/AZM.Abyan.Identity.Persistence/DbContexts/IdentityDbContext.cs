using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Entities.Base;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AZM.Abyan.Identity.Persistence.DbContexts;

public class IdentityDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public IdentityDbContext(
        DbContextOptions<IdentityDbContext> options,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<TenantUserRole> TenantUserRoles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TenantUserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new TenantConfiguration());
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Get userId from Claims and parse to Guid
        var currentUserId = _currentUserService.GetCurrentUserId();


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
