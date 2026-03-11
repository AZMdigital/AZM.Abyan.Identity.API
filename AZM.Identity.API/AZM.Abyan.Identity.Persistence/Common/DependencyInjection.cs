using AZM.Abyan.Identity.Domain.Entities;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Domain.Interfaces.GenericRepository;
using AZM.Abyan.Identity.Persistence.DbContexts;
using AZM.Abyan.Identity.Persistence.Repositories;
using AZM.Abyan.Identity.Persistence.Repositories.GenericRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AZM.Abyan.Identity.Persistence.Common;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        // Generic Repositories
        services.AddScoped<IRepository<Tenant, Guid>, Repository<Tenant, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<User, Guid>, Repository<User, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<Client, Guid>, Repository<Client, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<Role, Guid>, Repository<Role, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<Permission, Guid>, Repository<Permission, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<TenantUserRole, Guid>, Repository<TenantUserRole, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<TenantUserPermission, Guid>, Repository<TenantUserPermission, Guid, IdentityDbContext>>();
        // Added missing repositories
        services.AddScoped<IRepository<Scope, Guid>, Repository<Scope, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<Resource, Guid>, Repository<Resource, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<Policy, Guid>, Repository<Policy, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<License, Guid>, Repository<License, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<RefreshToken, Guid>, Repository<RefreshToken, Guid, IdentityDbContext>>();
        services.AddScoped<IRepository<LicenseClient, (Guid, Guid)>, Repository<LicenseClient, (Guid, Guid), IdentityDbContext>>();
        // Licensing specific repository
        services.AddScoped<ILicenseRepository, LicenseRepository>();

        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        return services;
    }
}

