using AZM.Abyan.Identity.Application.Common.Interfaces;
using AZM.Abyan.Identity.Application.Models;
using AZM.Abyan.Identity.Application.Services;
using AZM.Abyan.Identity.Domain.Interfaces;
using AZM.Abyan.Identity.Infrastructure.Security.Authorization;
using AZM.Abyan.Identity.Infrastructure.Services;
using AZM.Abyan.Identity.Persistence.Persistence.Repositories;
using AZM.Abyan.Identity.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZM.Abyan.Identity.Infrastructure.Common
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var keycloakSettings = configuration.GetSection("Keycloak").Get<KeycloakConfiguration>();
                var keycloakUrl = keycloakSettings?.BaseUrl ?? "http://localhost:8080";
                var realm = keycloakSettings?.Realm ?? "Abyan";

                options.Authority = $"{keycloakUrl}/realms/{realm}";
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = new[]
        {
                $"{keycloakUrl}/realms/{realm}",
                $"{keycloakUrl}/realms/master"
                        },
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
            });

            // Keycloak configuration - using KeycloakFormbuilder as default application config
            services.Configure<KeycloakConfiguration>(
                configuration.GetSection("KeycloakConfigurations:Tenants:Abyan:KeycloakFormbuilder"));

            // KeycloakConfigurations binding - binds the entire KeycloakConfigurations section
            services.Configure<KeycloakConfigurations>(
                configuration.GetSection("KeycloakConfigurations"));

            services.Configure<KeycloakOptions>(
                configuration.GetSection("Keycloak"));
            // Application Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            //services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IRealmAdminService, RealmAdminService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IPermissionSyncService, PermissionSyncService>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<ITenantRepository, TenantRepository>();
            services.AddScoped<IRealmResolverService, RealmResolverService>();
            // Sync Services
            services.AddScoped<ITenantSyncService, TenantSyncService>();
            services.AddScoped<IUserSyncService, UserSyncService>();
            services.AddScoped<IClientSyncService, ClientSyncService>();
            services.AddScoped<IRoleSyncService, RoleSyncService>();
            services.AddScoped<IPermissionKeycloakSyncService, PermissionKeycloakSyncService>();
            services.AddScoped<ITenantUserRoleSyncService, TenantUserRoleSyncService>();
            // Added missing sync services
            services.AddScoped<IScopeSyncService, ScopeSyncService>();
            services.AddScoped<IResourceSyncService, ResourceSyncService>();
            services.AddScoped<IPolicySyncService, PolicySyncService>();
            
            services.AddScoped<ISyncOrchestratorService, SyncOrchestratorService>();
  
            services.AddScoped<ITenantProvider, JwtTenantProvider>();

            services.AddScoped<IUmaAuthorizationService, KeycloakUmaAuthorizationService>();
            services.AddScoped<IUserPermissionQueryService, UserPermissionQueryService>();

            // HttpClient
            services.AddHttpClient<IKeycloakService, KeycloakService>((sp, client) =>
            {
                var config = sp.GetRequiredService<IOptions<KeycloakConfiguration>>().Value;
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(5);
            });
            return services;
        }
    }
}
